using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Solid3D
{
    public class Commands_Solid3D 
    {
        #region Make the cyclinder
        [CommandMethod("MakeCylinder")]
        public void makeCylinder()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            string getPointMessage = "Pick the center point: ";
            Point3d ptCenter = getPointWith(getPointMessage, ed);
            if (ptCenter.IsEqualTo(new Point3d(-1, -1, -1)))
            {
                return;
            }

            // Get the radius of the cylinder.
            string radiusMessage = "Pick the radius of cylinder: ";
            int radius = getValueWith(radiusMessage, ed);
            if (radius == -1)
            {
                return;
            }

            // Get the height of the cylinder. 
            string heightMessage = "Pick the height of the cylinder: ";
            int height = getValueWith(heightMessage, ed);
            if (height == -1)
            {
                return;
            }

            Database dwg = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction trans = dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blk = trans.GetObject(dwg.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (blk == null)
                    {
                        return;
                    }

                    Circle circle = new Circle(ptCenter, Vector3d.ZAxis, radius);

                    // make region.
                    DBObjectCollection dbObjCollec = new DBObjectCollection();
                    dbObjCollec.Add(circle);

                    DBObjectCollection regionObjCollec = Region.CreateFromCurves(dbObjCollec);

                    Solid3d solid3d = new Solid3d();
                    solid3d.Extrude((Region)regionObjCollec[0], height, 0.0);

                    // append elements into database. 
                    BlockTableRecord blkRecord = trans.GetObject(blk[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    blkRecord.AppendEntity(solid3d);

                    trans.AddNewlyCreatedDBObject(solid3d, true);

                    trans.Commit();
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    throw;
                }
            }
        }


        private Point3d getPointWith(string message, Editor ed)
        {
            PromptPointOptions ptPointOpt = new PromptPointOptions(message);
            PromptPointResult ptPointRes = ed.GetPoint(ptPointOpt);
            Point3d ptUCS = new Point3d(-1, -1, -1);

            if (ptPointRes.Status != PromptStatus.OK)
            {
                return ptUCS;
            }

            // Turn the point at UCS into the point at WCS.
            ptUCS = ptPointRes.Value;
            Point3d ptWCS = ptUCS.TransformBy(ed.CurrentUserCoordinateSystem);

            return ptWCS;
        }

        private int getValueWith(string message, Editor ed)
        {
            PromptIntegerOptions ptIntegerOpt = new PromptIntegerOptions(message);
            PromptIntegerResult ptIntegerRes = ed.GetInteger(ptIntegerOpt);

            if (ptIntegerRes.Status != PromptStatus.OK)
            {
                return -1;
            }

            int radius = ptIntegerRes.Value;

            return radius;
        }
        #endregion


        #region Find interference between solids
        [CommandMethod("InterferenceSolids")]
        public void interferenceSolids()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blkTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (blkTable == null)
                    {
                        trans.Abort();
                        return;
                    }

                    BlockTableRecord blkTableRecord = trans.GetObject(blkTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (blkTableRecord == null)
                    {
                        return;
                    }

                    using (Solid3d solid3DBox = new Solid3d())
                    {
                        // 3D Solid Box.
                        solid3DBox.CreateBox(5, 7, 10);
                        solid3DBox.ColorIndex = 7;

                        // Position of the center of solid3DBox at (5, 5, 0)
                        solid3DBox.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) - Point3d.Origin));

                        //blkTableRecord.AppendEntity(solid3DBox);
                        //trans.AddNewlyCreatedDBObject(solid3DBox, true);

                        // 3D Solid Cylinder. 
                        using (Solid3d solid3DCylinder = new Solid3d())
                        {
                            solid3DCylinder.CreateFrustum(20, 5, 5, 5);
                            solid3DCylinder.ColorIndex = 4;

                            //blkTableRecord.AppendEntity(solid3DCylinder);
                            //trans.AddNewlyCreatedDBObject(solid3DCylinder, true);

                            // Create 3D solid from the interference of the box and cylinder. 
                            //Solid3d solid3dCopy = solid3DCylinder.Clone() as Solid3d;

                            //if (solid3dCopy.CheckInterference(solid3DBox) == true)
                            //{
                            //    solid3dCopy.BooleanOperation(BooleanOperationType.BoolSubtract, solid3DBox.Clone() as Solid3d);
                            //    solid3dCopy.ColorIndex = 1;
                            //}

                            //// add solid3dCopy to the block table record. 
                            //blkTableRecord.AppendEntity(solid3dCopy);
                            //trans.AddNewlyCreatedDBObject(solid3dCopy, true);

                            Solid3d solid3dCopyCylinder = solid3DCylinder.Clone() as Solid3d;

                            if (solid3dCopyCylinder.CheckInterference(solid3DBox) == true)
                            {
                                solid3dCopyCylinder.BooleanOperation(BooleanOperationType.BoolIntersect, solid3DBox);
                                solid3dCopyCylinder.ColorIndex = 3;
                            }

                            blkTableRecord.AppendEntity(solid3dCopyCylinder);
                            trans.AddNewlyCreatedDBObject(solid3dCopyCylinder, true);
                        }
                    }

                    trans.Commit();
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    throw;
                }
            }

        }

        #endregion
    }
}
