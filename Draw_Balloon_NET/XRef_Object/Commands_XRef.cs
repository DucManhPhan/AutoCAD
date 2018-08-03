using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace XRef_Object
{
    public class Commands_XRef
    {
        #region XRef's usage       
        [CommandMethod("UseXRefs")]
        public void useXRefs()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    string modeXRefMessage = "Do you want to choose the intersection of master and xref - 1 or xref and xref - 0: ";
                    int choice = getValueWith(modeXRefMessage, ed);
                    if (choice > 1 || choice < 0)
                    {
                        return;
                    }

                    switch (choice)
                    {
                        case 0:
                            implementDoubleXRef(db, ed);
                            break;

                        case 1:
                            implementMasterAndXRef(db, ed);
                            break;

                        default:
                            break;
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

        private string getPathOfEntitiesInXRef(Editor ed)
        {
            if (ed == null)
            {
                return null;
            }

            string path = "";

            PromptOpenFileOptions ptOpenFileOpt = new PromptOpenFileOptions("Choose file that you want: ");
            ptOpenFileOpt.Filter = "Drawing (*.dwg)|*.dwg";

            PromptFileNameResult ptFileNameRes = ed.GetFileNameForOpen(ptOpenFileOpt);
            if (ptFileNameRes.Status != PromptStatus.OK)
            {
                return path;
            }

            path = ptFileNameRes.StringResult;

            return path;
        }

        private void implementMasterAndXRef(Database db, Editor ed)
        {
            if (db == null || ed == null)
            {
                return;
            }

            // make the Block Reference object and insert to database. 
            insertXRefIntoDatabase(db, ed);

            // select the Block Reference object, and then explode it.
            BlockReference blkReferRes = getBlockRefer(db, ed);
            if (blkReferRes == null)
            {
                return;
            }

            DBObjectCollection dbObjCol = new DBObjectCollection();
            blkReferRes.Explode(dbObjCol);

            // select the entity = Circle. 
            Circle cir = getSelectedObject(db, ed) as Circle;
            if (cir == null)
            {
                return;
            }

            // find the intersection of master and this object.    
            Point3dCollection pts3D = null;
            foreach (Entity ent in dbObjCol)
            {
                pts3D = new Point3dCollection();
                cir.IntersectWith(ent, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);

                foreach (Point3d pt in pts3D)
                {
                    ed.WriteMessage("Point number: " + pt.X + " " + pt.Y + " " + pt.Z);
                    markIntersectionPoint(db, ed, pt);
                }
            }
        }

        private void implementDoubleXRef(Database db, Editor ed)
        {
            if (db == null || ed == null)
            {
                return;
            }

            const int NumberEntity = 2;

            List<BlockReference> lstBlkRefer = new List<BlockReference>();
            List<DBObjectCollection> lstDBObjCol = new List<DBObjectCollection>();

            // insert objects into Database.
            BlockReference blkReferRes = null;
            for (int i = 0; i < NumberEntity; ++i)
            {
                insertXRefIntoDatabase(db, ed);
            }

            // select the block reference objects have been just created. 
            for (int i = 0; i < NumberEntity; ++i)
            {
                blkReferRes = getBlockRefer(db, ed);
                if (blkReferRes == null)
                {
                    return;
                }

                DBObjectCollection dbObjCol = new DBObjectCollection();
                blkReferRes.Explode(dbObjCol);

                lstDBObjCol.Add(dbObjCol);
            }

            // find the intersection point.
            foreach (Entity ent_FirstBlkRef in lstDBObjCol[0])
            {
                Point3dCollection pt3DCol = new Point3dCollection();
                foreach (Entity ent_SecondBlkRef in lstDBObjCol[1])
                {
                    ent_FirstBlkRef.IntersectWith(ent_SecondBlkRef, Intersect.OnBothOperands, pt3DCol, IntPtr.Zero, IntPtr.Zero);

                    foreach (Point3d pt in pt3DCol)
                    {
                        ed.WriteMessage("Point number: " + pt.X + " " + pt.Y + " " + pt.Z);
                        markIntersectionPoint(db, ed, pt);
                    }
                }
            }
        }

        private void insertXRefIntoDatabase(Database db, Editor ed)
        {
            if (db == null || ed == null)
            {
                return;
            }

            // get the path of XRef file.
            string pathXRef = getPathOfEntitiesInXRef(ed);
            if (!File.Exists(pathXRef))
            {
                return;
            }

            string nameXRef = "";
            if (!String.IsNullOrEmpty(pathXRef))
            {
                nameXRef = Path.GetFileNameWithoutExtension(pathXRef);
            }

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId objID = db.AttachXref(pathXRef, nameXRef);
                    if (objID.IsValid)
                    {
                        BlockTableRecord blkTableRecord = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        // make the block reference for this entity of xref. 
                        BlockReference blkRefer = new BlockReference(Point3d.Origin, objID);

                        blkTableRecord.AppendEntity(blkRefer);
                        trans.AddNewlyCreatedDBObject(blkRefer, true);
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

        private BlockReference getBlockRefer(Database db, Editor ed)
        {
            if (db == null || ed == null)
            {
                return null;
            }

            BlockReference blkRefer = null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    PromptEntityOptions ptEntityOpt = new PromptEntityOptions("Select the Block Reference object that you have just inserted:");
                    PromptEntityResult ptEntityRes = ed.GetEntity(ptEntityOpt);
                    if (ptEntityRes.Status != PromptStatus.OK)
                    {
                        return null;
                    }

                    //Get this entity
                    blkRefer = trans.GetObject(ptEntityRes.ObjectId, OpenMode.ForRead) as BlockReference;
                    if (blkRefer != null)
                    {
                        return blkRefer;
                    }
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    throw;
                }
            }

            return null;
        }

        private Entity getSelectedObject(Database db, Editor ed)
        {
            if (db == null || ed == null)
            {
                return null;
            }

            Entity ent = null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                PromptEntityOptions ptEntityOpt = new PromptEntityOptions("Pick the entity that you want: ");
                PromptEntityResult ptEntityRes = ed.GetEntity(ptEntityOpt);
                if (ptEntityRes.Status != PromptStatus.OK)
                {
                    return null;
                }

                ent = trans.GetObject(ptEntityRes.ObjectId, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    return ent;
                }
            }

            return null;
        }


        private void markIntersectionPoint(Database db, Editor ed, Point3d pt3D)
        {
            if (db == null || ed == null)
            {
                return;
            }


            const int radius = 1;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blk = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (blk == null)
                    {
                        trans.Abort();
                        return;
                    }

                    // make the circle. 
                    Circle cir = new Circle();
                    cir.Center = pt3D;
                    cir.Radius = radius;

                    // save this circle to database. 
                    BlockTableRecord blkRecord = trans.GetObject(blk[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    blkRecord.AppendEntity(cir);

                    trans.AddNewlyCreatedDBObject(cir, true);

                    trans.Commit();
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    throw;
                }
            }
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
    }
}
