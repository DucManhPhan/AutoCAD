using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Draw_Balloon_net
{
    #region Entities Jig    
    #region Circle Jig
    public class CircleJig : EntityJig
    {
        private Point3d centerPoint;
        private double radius;

        private int currentInputValue;
        public int CurrentInput
        {
            get
            {
                return currentInputValue;
            }

            set
            {
                if (currentInputValue != value)
                {
                    currentInputValue = value;
                }
            }
        }

        public CircleJig(Entity entity) : base(entity)
        {
            // nothing to do.
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (CurrentInput)
            {
                // get the center point of the circle. 
                case 0:
                    Point3d oldPnt = centerPoint;
                    PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick the center point: ");

                    if (jigPromptResult.Status == PromptStatus.OK)
                    {
                        centerPoint = jigPromptResult.Value;

                        if (oldPnt.DistanceTo(centerPoint) < 0.0001)
                        {
                            return SamplerStatus.NoChange;
                        }
                    }

                    break;

                // get the radius of the circle. 
                case 1:
                    double oldRadius = radius;
                    JigPromptDistanceOptions jigPromptDistance = new JigPromptDistanceOptions("Pick the radius: ");

                    jigPromptDistance.UseBasePoint = true;
                    jigPromptDistance.BasePoint = centerPoint;

                    PromptDoubleResult jigPromptRadiusResult = prompts.AcquireDistance(jigPromptDistance);
                    if (jigPromptRadiusResult.Status == PromptStatus.OK)
                    {
                        radius = jigPromptRadiusResult.Value;

                        if (Math.Abs(radius) < 0.01)
                        {
                            radius = 1;
                        }

                        if (Math.Abs(oldRadius - radius) < 0.0001)
                        {
                            return SamplerStatus.NoChange;
                        }
                    }

                    break;
            }

            return SamplerStatus.OK;
        }


        protected override bool Update()
        {
            Circle circle = (Circle)Entity;

            switch (CurrentInput)
            {
                case 0:
                    circle.Center = centerPoint;

                    break;

                case 1:
                    circle.Radius = radius;

                    break;
            }

            return true;
        }
    }



    #endregion


    #region Balloon Jig
    public class BalloonJig : DrawJig //EntityJig 
    {
        const double DefaultSize = 10;

        private Line _line;
        private Circle _circle;

        public BalloonJig() : base()
        {
            // nothing to do.
        }

        public BalloonJig(Line line, Circle circle) : base()
        {
            _line = line;
            _circle = circle;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            // get the center point of circle. 
            JigPromptPointOptions jigPrptPointOpt = new JigPromptPointOptions("Pick the center point of circle: ");
            PromptPointResult ptPointRes = prompts.AcquirePoint(jigPrptPointOpt);

            if (ptPointRes.Status != PromptStatus.OK)
            {
                return SamplerStatus.Cancel;
            }

            _circle.Center = ptPointRes.Value;
            _circle.Radius = DefaultSize;

            // calculate the end point of line.
            Vector3d vec = _line.StartPoint - _circle.Center;
            vec = vec.GetNormal();
            _line.EndPoint = _circle.Center + vec * DefaultSize;

            return SamplerStatus.OK;
        }


        protected override bool WorldDraw(WorldDraw draw)
        {
            draw.Geometry.Draw(_line);
            draw.Geometry.Draw(_circle);

            return true;
        }
    }
    #endregion
    #endregion


    #region Commands
    public class Commands
    {
        #region Insert Balloon Command
        [CommandMethod("insertBalloon")]
        public void insertBalloon()
        {
            Settings setting = Settings.getInstance();

            // make the line and circle.
            Line _lineBal = new Line();
            Circle _circleBal = new Circle();

            // set properties for line and circle.
            _lineBal.ColorIndex = setting.IndexColorLine;
            _circleBal.ColorIndex = setting.IndexColorCircle;
            _circleBal.Diameter = setting.Diameter;

            // get the first point. 
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions ptPointOpt = new PromptPointOptions("Pick the first point: ");
            PromptPointResult ptPointRes = ed.GetPoint(ptPointOpt);

            if (ptPointRes.Status != PromptStatus.OK)
            {
                return;
            }

            // Turn the point at UCS into the point at WCS.
            Point3d ptUCS = ptPointRes.Value;
            _lineBal.StartPoint = ptUCS.TransformBy(ed.CurrentUserCoordinateSystem);

            // Jig action.
            BalloonJig jigBalloon = new BalloonJig(_lineBal, _circleBal);
            PromptResult prompt = ed.Drag(jigBalloon);

            if (prompt.Status == PromptStatus.Cancel || prompt.Status == PromptStatus.Error)
            {
                return;
            }

            // save all of elements into Database. 
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

                    // make text for circle. 
                    DBText _textBal = makeText(_circleBal.Center);
                    _textBal.ColorIndex = setting.IndexColorText;
                    _textBal.TextString = setting.Text;

                    // make the list of objectId of each elements. 
                    List<ObjectId> lstObjectId = new List<ObjectId>();

                    // append elements into database. 
                    BlockTableRecord blkRecord = trans.GetObject(blk[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    ObjectId objLineId = blkRecord.AppendEntity(_lineBal);
                    lstObjectId.Add(objLineId);

                    ObjectId objCircleId = blkRecord.AppendEntity(_circleBal);
                    lstObjectId.Add(objCircleId);

                    ObjectId objTextId = blkRecord.AppendEntity(_textBal);
                    lstObjectId.Add(objTextId);

                    // save id of each elements. 
                    ImplementationDatabase.LstObjectId = lstObjectId;

                    trans.AddNewlyCreatedDBObject(_lineBal, true);
                    trans.AddNewlyCreatedDBObject(_circleBal, true);
                    trans.AddNewlyCreatedDBObject(_textBal, true);

                    trans.Commit();
                }
                catch (System.Exception)
                {
                    trans.Abort();
                    throw;
                }                
            }

            // get name for this group.
            PromptStringOptions strOptions = new PromptStringOptions("The name of this entity: ");
            strOptions.AllowSpaces = true;

            PromptResult promptRes = ed.GetString(strOptions);
            if (promptRes.Status != PromptStatus.OK)
            {
                ImplementationDatabase.NameGroup = "*";
            }
            else
            {
                ImplementationDatabase.NameGroup = promptRes.StringResult;
            }

            // make group.
            ImplementationDatabase.makeGroup();
        }

        DBText makeText(Point3d position)
        {
            DBText txt = new DBText();
            txt.Position = position;
            txt.Height = 0.5;
            txt.TextString = "A";

            return txt;
        }

        #endregion


        #region Balloon Settings
        [CommandMethod("settingBalloon")]
        public void callSettingBalloon()
        {
            // get all of the selected group.
            ImplementationDatabase.LstGroup = ImplementationDatabase.getSelectedGroups();

            Settings setting = Settings.getInstance();

            BalloonSettingDialog balloonDialog = new BalloonSettingDialog();
            balloonDialog.ShowDialog();
        }
        #endregion


        #region Circle command
        [CommandMethod("makeCircle")]
        public void makeCircle()
        {
            Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);
            CircleJig jigCir = new CircleJig(circle);

            for (int i = 0; i < 2; ++i)
            {
                jigCir.CurrentInput = i;
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptResult prompt = ed.Drag(jigCir);

                if (prompt.Status == PromptStatus.Cancel || prompt.Status == PromptStatus.Error)
                {
                    return;
                }
            }

            Database dwg = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction trans = dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    // use the transaction to open these objects.
                    // the transaction will automatically dispose these objects when done
                    // so we don't have to worry about manually disposing them.
                    BlockTable blk = trans.GetObject(dwg.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (blk == null)
                    {
                        return;
                    }

                    BlockTableRecord blkRecord = trans.GetObject(blk[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    blkRecord.AppendEntity(circle);

                    trans.AddNewlyCreatedDBObject(circle, true);
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


        #region Intersection of the two entities. 
        enum State
        {
            LineType, CircleType
        }

        [CommandMethod("intersectEntities")]
        public void intersectEntities()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            Line firstLine = null;
            Line secondLine = null;
            Circle cir = null;
            Entity ent = null;
            State st = State.LineType;
            
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    //Select first polyline
                    PromptEntityOptions ptEntityOpt = new PromptEntityOptions("Select firtst line:");
                    PromptEntityResult ptEntityRes = ed.GetEntity(ptEntityOpt);
                    if (ptEntityRes.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    //Get the polyline entity
                    ent = (Entity)trans.GetObject(ptEntityRes.ObjectId, OpenMode.ForRead);
                    if (ent is Line)
                    {
                        firstLine = ent as Line;
                    }

                    //Select 2nd polyline
                    ptEntityOpt = new PromptEntityOptions("\n Select the second line or the circle:");
                    ptEntityRes = ed.GetEntity(ptEntityOpt);
                    if (ptEntityRes.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    ent = (Entity)trans.GetObject(ptEntityRes.ObjectId, OpenMode.ForRead);
                    if (ent is Line)
                    {
                        secondLine = ent as Line;
                    }
                    else if (ent is Circle)
                    {
                        cir = ent as Circle;
                        st = State.CircleType;
                    }

                    Point3dCollection pts3D = new Point3dCollection();

                    //Get the intersection Points.
                    switch (st)
                    {
                        case State.LineType:
                            firstLine.IntersectWith(secondLine, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);
                            break;

                        case State.CircleType:
                            firstLine.IntersectWith(cir, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);
                            break;

                        default:
                            break;
                    }

                    foreach (Point3d pt in pts3D)
                    {
                        ed.WriteMessage("Point number: " + pt.X + " " + pt.Y + " " + pt.Z);
                        //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("\n Intersection Point: " + "\nX = " + pt.X + "\nY = " + pt.Y + "\nZ = " + pt.Z);
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
                catch(System.Exception)
                {
                    trans.Abort();
                    throw;
                }                                
            }

        }

        #endregion
    }
    #endregion


    #region Communicate with Database
    public static class ImplementationDatabase
    {
        public static string NameGroup;
        private const string IdentifyBalloon = "Balloon";
        public static List<Group> LstGroup = null;

        #region Group elements
        public static void makeGroup()
        {
            Database currentDB = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction trans = currentDB.TransactionManager.StartTransaction())
            {
                // get object DBDictionary and make the Group object.
                DBDictionary dbDict = trans.GetObject(currentDB.GroupDictionaryId, OpenMode.ForWrite) as DBDictionary;
                Group dbGroup = new Group(NameGroup, true);
                dbGroup.Description = IdentifyBalloon;        // it is identification of balloon's type.

                // add group to Group Dictionary. 
                dbDict.UpgradeOpen();
                dbDict.SetAt(NameGroup, dbGroup);

                foreach (ObjectId id in LstObjectId)
                {
                    dbGroup.Append(id);
                }

                trans.AddNewlyCreatedDBObject(dbGroup, true);
                trans.Commit();
            }
        }


        public static List<ObjectId> LstObjectId = null;
        #endregion


        #region Get all of group Dictionary in Database
        public static ObservableCollection<string> getGroups()
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dbDict = trans.GetObject(db.GroupDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (dbDict == null)
                {
                    return null;
                }

                // get the name of all layer tables. 
                ObservableCollection<string> obsCollect = new ObservableCollection<string>();
                foreach (DBDictionaryEntry entry in dbDict)
                {
                    Group gp = trans.GetObject(entry.Value, OpenMode.ForRead) as Group;
                    if (gp == null)
                    {
                        continue;
                    }

                    obsCollect.Add(gp.Name);
                }

                return obsCollect;
            }
        }
        #endregion


        #region Get all of Layer's name
        public static ObservableCollection<string> getLayers()
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable layTable = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (layTable == null)
                {
                    return null;
                }

                // get the name of all layer tables. 
                ObservableCollection<string> obsCollect = new ObservableCollection<string>();
                foreach (ObjectId id in layTable)
                {
                    LayerTableRecord layTblRecord = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                    if (layTblRecord == null)
                    {
                        continue;
                    }

                    obsCollect.Add(layTblRecord.Name);
                }

                return obsCollect;
            }
        }

        #endregion       


        #region Set values for all of groups in Database
        public static void updateValueForAllOfElements()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dbDict = trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite) as DBDictionary;
                if (dbDict == null)
                {
                    return;
                }

                // get the name of all layer tables. 
                ObservableCollection<string> obsCollect = new ObservableCollection<string>();
                foreach (DBDictionaryEntry entry in dbDict)
                {
                    Group gp = trans.GetObject(entry.Value, OpenMode.ForWrite) as Group;
                    if (gp == null || gp.Description != IdentifyBalloon)
                    {
                        continue;
                    }

                    updateElements(gp);
                }

                trans.Commit();
            }
        }


        #region Update data for elements in one group
        public static void updateElements(Group gp)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // update properties for entities. 
            Settings setting = Settings.getInstance();

            ObjectId[] objIdArr = gp.GetAllEntityIds();
            Line line = null;
            Circle circle = null;
            DBText text = null;
            DBObject dbObj = null;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in objIdArr)
                {
                    dbObj = trans.GetObject(id, OpenMode.ForWrite);

                    if (id.ObjectClass.DxfName == "LINE")
                    {
                        line = dbObj as Line;
                        line.ColorIndex = setting.IndexColorLine;
                    }
                    else if (id.ObjectClass.DxfName == "CIRCLE")
                    {
                        circle = dbObj as Circle;
                        circle.Diameter = setting.Diameter;
                        circle.ColorIndex = setting.IndexColorCircle;
                    }
                    else if (id.ObjectClass.DxfName == "TEXT")
                    {
                        text = dbObj as DBText;
                        text.TextString = setting.Text;
                        text.ColorIndex = setting.IndexColorText;
                    }
                }

                // change point of circle.
                changeAllOfPoint(circle, line);

                trans.Commit();
            }
        }


        public static void changeAllOfPoint(Circle circle, Line line)
        {
            if (circle == null || line == null)
            {
                return;
            }

            // calculate the end point of line.
            Vector3d vec = line.StartPoint - circle.Center;
            vec = vec.GetNormal();
            line.EndPoint = circle.Center + (vec * (circle.Diameter / 2));
        }
        #endregion
        #endregion


        #region Set value for selected groups in Database 
        #region Get selected groups in AutoCAD. 
        public static List<Group> getSelectedGroups()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            List<Group> lstGroup = null;

            PromptSelectionResult selectionResult = ed.SelectImplied();
            if (selectionResult.Status == PromptStatus.Error)
            {
                PromptSelectionOptions prmptSelectOpt = new PromptSelectionOptions();
                prmptSelectOpt.MessageForAdding = "Pick you entities that you want to change : ";

                selectionResult = ed.GetSelection(prmptSelectOpt);
            }
            else
            {
                // pickfirst
                ed.SetImpliedSelection(new ObjectId[0]);
            }

            if (selectionResult.Status == PromptStatus.OK)
            {                
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    SelectionSet selectedEntities = selectionResult.Value;
                    lstGroup = new List<Group>();

                    foreach (ObjectId id in selectedEntities.GetObjectIds())
                    {
                        Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;

                        // get group that correspond to this "ent".
                        ObjectIdCollection ids = ent.GetPersistentReactorIds();

                        foreach (ObjectId objId in ids)
                        {
                            DBObject objDB = trans.GetObject(objId, OpenMode.ForWrite);
                            if (objDB is Group)
                            {
                                Group grp = objDB as Group;
                                if (lstGroup.Contains(grp))
                                {
                                    continue;
                                }

                                lstGroup.Add(grp);
                            }
                        }
                    }
                }
            }
            return lstGroup;
        }
        #endregion


        #region set value for selected groups
        public static void updateValueForSelectedElements()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //List<Group> lstGroup = getSelectedGroups();                
                if (LstGroup == null)
                {
                    return;
                }

                foreach (Group grp in LstGroup)
                {
                    if (grp.Description != IdentifyBalloon)
                    {
                        continue;
                    }

                    updateElements(grp);
                }

                trans.Commit();
            }
        }

        #endregion

        #endregion
    }
    #endregion
}
