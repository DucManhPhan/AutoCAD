#include "stdafx.h"
#include "AcEdInsertBalloonJig.h"


#pragma region Constructor
AcEdInsertBalloonJig::AcEdInsertBalloonJig(const AcGePoint3d& pt1) : m_pt3dFirstOfLine(pt1), m_nRadius(10)
{
	m_pCustomEntity = new AcDbBalloonEntity();
}

AcEdInsertBalloonJig::AcEdInsertBalloonJig(const AcGePoint3d& pt1, int radius)
							: m_pt3dFirstOfLine(pt1)
							, m_nRadius(radius)
{
	// nothing to do.
}
#pragma endregion


#pragma region Destructor
AcEdInsertBalloonJig::~AcEdInsertBalloonJig()
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
AcDbBalloonEntity* AcEdInsertBalloonJig::getCustomEntity()
{
	return m_pCustomEntity;
}


// save the entities into graphical database. 
AcDbObjectId AcEdInsertBalloonJig::append(AcDbDatabase* pDb, const ACHAR* pDbSpace)
{	
	std::vector<AcDbEntity*> vtEntities = m_pCustomEntity->getEntities();
	int size = vtEntities.size();	
	
	// write to database.
	pushEntityToDB(pDb, pDbSpace, m_pCustomEntity);
	m_pCustomEntity->close();

	return m_pCustomEntity->id();
}


void AcEdInsertBalloonJig::pushEntityToDB(AcDbDatabase* pDb, const ACHAR* pDbSpace, AcDbEntity* pEntity)
{
	// Case 1: this way will save all entities before grouping.
	if (pEntity == nullptr)
	{
		return;
	}

	// check pointer to object AcDbDatabase (graphical database).
	if (pDb == nullptr)
	{
		pDb = acdbHostApplicationServices()->workingDatabase();
	}

	// get object Block Table.
	AcDbBlockTable* pBlockTable = nullptr; 
	if (pDb->getBlockTable(pBlockTable, AcDb::kForRead) != Acad::eOk)
	{
		return;
	}

	// get object BlockTable record. 
	AcDbBlockTableRecord* pRecord = nullptr;
	if (pBlockTable != nullptr)
	{		
		if (pBlockTable->getAt(pDbSpace, pRecord, AcDb::kForWrite) != Acad::eOk)
		{
			return;
		}

		// append pEntity object to the above record.
		//AcDbObjectId obID;
		pRecord->appendAcDbEntity(pEntity);		

		// close the record, blocktable, objectEntity.
		pBlockTable->close();
		pRecord->close();
	}


	// Case 2: This way will make a group, and then save them to graphical database.
	//m_pCustomEntity->groups();
}


void AcEdInsertBalloonJig::makeBalloon()
{
	AcEdJig::DragStatus stat;

	setDispPrompt(_T("Enter center point of Circle: "));
	stat = drag();		// make the drag loop.

	append();
}


AcEdJig::DragStatus AcEdInsertBalloonJig::sampler()
{
	DragStatus stat; 
	setUserInputControls((UserInputControls)
		( AcEdJig::kAccept3dCoordinates
		| AcEdJig::kNoNegativeResponseAccepted
		| AcEdJig::kNoZeroResponseAccepted));
	
	stat = acquirePoint(m_pt3dCenter);
	static AcGePoint3d ptTemp;

	if (m_pt3dCenter != ptTemp)
	{		
		// calculate the second point of the line.
		AcGeVector3d vec = m_pt3dFirstOfLine - m_pt3dCenter;
		vec.normalize();
		m_pt3dSecondOfLine = m_pt3dCenter + RADIUS * vec;

		ptTemp = m_pt3dCenter;

		return AcEdJig::kNormal;
	}
	else if (stat == AcEdJig::kNormal)
	{
		return AcEdJig::kNoChange;
	}

	return stat;
}


// stores or update each point.
Adesk::Boolean AcEdInsertBalloonJig::update()
{
	m_pCustomEntity->updateInfor(*this);	
	return Adesk::kTrue;
}


// return to the entites, and then call the worldDraw() function to draw entities.
AcDbEntity* AcEdInsertBalloonJig::entity() const
{
	return (AcDbEntity*)m_pCustomEntity;
}
#pragma endregion