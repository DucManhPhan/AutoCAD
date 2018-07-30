#include "stdafx.h"
#include "AcDbBalloonEntity.h"
#include "AcEdInsertBalloonJig.h"


ACRX_DXF_DEFINE_MEMBERS(
	AcDbBalloonEntity, AcDbEntity,
	AcDb::kDHL_CURRENT, AcDb::kMReleaseCurrent,
	AcDbProxyEntity::kNoOperation, ACDBBALLOONENTITY,
	"ACDBBalloonEntity"
)
//}}AFX_ARX_MACRO



#pragma region Constructor && Destructor
AcDbBalloonEntity::AcDbBalloonEntity()
{
	// nothing to do.
}


AcDbBalloonEntity::~AcDbBalloonEntity()
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
std::vector<AcDbEntity*> AcDbBalloonEntity::getEntities()
{
	return m_vtEntities;
}


void AcDbBalloonEntity::setEntities(std::vector<AcDbEntity*>&& vtEntities)
{
	m_vtEntities = vtEntities;
}


std::vector<AcDbObjectId> AcDbBalloonEntity::getObjectIds()
{
	return m_vtObjectIds;
}


void AcDbBalloonEntity::setObjectIds(std::vector<AcDbObjectId>&& vtObjectIds)
{
	m_vtObjectIds = vtObjectIds;
}


void AcDbBalloonEntity::updateInfor(const AcEdInsertBalloonJig& jig)
{
	AcGePoint3d firstPoint = jig.getFirstPointLine();
	AcGePoint3d secondPoint = jig.getSecondPointLine();
	AcGePoint3d centerPoint = jig.getCenterPoint();

	int size = m_vtEntities.size();

	for (int i = 0; i < size; ++i)
	{
		if (m_vtEntities[i]->isKindOf(AcDbSelfLine::desc()))
		{
			AcDbSelfLine* pLine = AcDbSelfLine::cast(m_vtEntities[i]);
			if (pLine)
			{
				pLine->updateInforLine(firstPoint, secondPoint);
			}
		}
		else if (m_vtEntities[i]->isKindOf(AcDbSelfCircle::desc()))
		{
			AcDbSelfCircle* pCircle = AcDbSelfCircle::cast(m_vtEntities[i]);
			if (pCircle)
			{
				pCircle->updateInforCircle(centerPoint);
			}
		}
	}
}


Adesk::Boolean AcDbBalloonEntity::subWorldDraw(AcGiWorldDraw *wd)
{
	assertReadEnabled();

	// draw each entity of Custom Entity.
	int size = m_vtEntities.size();
	for (int i = 0; i < size; ++i)
	{
		AcDbEntity* pEntity = m_vtEntities[i];
		if (pEntity)
		{
			wd->geometry().draw(pEntity);
		}
	}	

	return Adesk::kTrue;
}


void AcDbBalloonEntity::groups()
{
	AcDbGroup *pGroup = new AcDbGroup();
	if (!pGroup)
	{
		return;
	}

	AcDbDictionary *pGroupDict = nullptr;

	// get graphical database and Group Dictionary.
	AcDbDatabase* pDb = acdbHostApplicationServices()->workingDatabase();
	if (!pDb)
	{
		return;
	}

	if (pDb->getGroupDictionary(pGroupDict, AcDb::kForWrite) != Acad::eOk)
	{
		return;
	}

	// get Group.
	AcDbObjectId groupId;
	pGroupDict->setAt(L"*", pGroup, groupId);

	pGroupDict->close();
	pGroup->close();

	makeGroup(groupId);	
}


void AcDbBalloonEntity::makeGroup(const AcDbObjectId& groupId)
{
	AcDbGroup *pGroup = nullptr;
	if (acdbOpenObject(pGroup, groupId, AcDb::kForWrite) == Acad::eOk)
	{
		int nLength = m_vtObjectIds.size();
		for (int i = 0; i < nLength; i++)
		{			
			pGroup->append(m_vtObjectIds[i]);				
		}

		pGroup->close();
	}
}


void AcDbBalloonEntity::setSizeBalloon(int size)
{
	// notify the changes for AutoCAD.
	assertWriteEnabled();	

	// write information to AutoCAD.
	AcDbSelfCircle* pCircle = nullptr;
	AcDbSelfLine* pLine = nullptr;

	int nLength = m_vtEntities.size();
	for (int i = 0; i < nLength; ++i)
	{
		if (m_vtEntities[i] && m_vtEntities[i]->isKindOf(AcDbSelfCircle::desc()))
		{
			pCircle = AcDbSelfCircle::cast(m_vtEntities[i]);
		}
		else if (m_vtEntities[i] && m_vtEntities[i]->isKindOf(AcDbSelfLine::desc()))
		{
			pLine = AcDbSelfLine::cast(m_vtEntities[i]);
		}
	}

	if (pCircle && pLine)
	{
		AcGePoint3d secondPoint;
		AcGeVector3d vec = pLine->startPoint() - pCircle->center();
		vec.normalize();
		secondPoint = pCircle->center() + (size * vec);

		pLine->setEndPoint(secondPoint);
		pCircle->setRadius(size);
	}
}


void AcDbBalloonEntity::setColorBalloon(int color)
{
	// notify the changes for AutoCAD.
	assertWriteEnabled();

	// write information to AutoCAD.
	int size = m_vtEntities.size();
	for (int i = 0; i < size; ++i)
	{
		if (m_vtEntities[i])
		{
			m_vtEntities[i]->setColorIndex(color);
		}
	}
}


void AcDbBalloonEntity::setTextBalloon(tstring strText)
{
	// notify the changes for AutoCAD.
	assertWriteEnabled();

	// write information to AutoCAD.
	int size = m_vtEntities.size();
	for (int i = 0; i < size; ++i)
	{
		if (m_vtEntities[i] && m_vtEntities[i]->isKindOf(AcDbSelfCircle::desc()))
		{
			AcDbSelfCircle* pCircle = AcDbSelfCircle::cast(m_vtEntities[i]);
			if (pCircle)
			{
				pCircle->setInnerText(strText);
			}
		}
	}
}


Acad::ErrorStatus AcDbBalloonEntity::subGetOsnapPoints(AcDb::OsnapMode osnapMode,
													Adesk::GsMarker gsSelectionMark,
													const AcGePoint3d&  pickPoint,
													const AcGePoint3d&  lastPoint,
													const AcGeMatrix3d& viewXform,
													AcGePoint3dArray&   snapPoints,
													AcDbIntArray &   geomIds)
{
	assertReadEnabled();
	
	// find some special points in Balloon.
	AcGePoint3d pt3dMidLine;
	AcGePoint3d pt3dFirstLine;
	AcGePoint3d pt3dSecondLine;
	AcGePoint3d pt3dCenter;

	int size = m_vtEntities.size();

	for (int i = 0; i < size; ++i)
	{
		if (m_vtEntities[i]->isKindOf(AcDbSelfLine::desc()))
		{
			AcDbSelfLine* pLine = AcDbSelfLine::cast(m_vtEntities[i]);
			if (pLine)
			{
				pt3dFirstLine = pLine->startPoint();
				pt3dSecondLine = pLine->endPoint();

				pt3dMidLine.x = (pt3dFirstLine.x + pt3dSecondLine.x) / 2;
				pt3dMidLine.y = (pt3dFirstLine.y + pt3dSecondLine.y) / 2;
			}
		}
		else if (m_vtEntities[i]->isKindOf(AcDbSelfCircle::desc()))
		{
			AcDbSelfCircle* pCircle = AcDbSelfCircle::cast(m_vtEntities[i]);
			if (pCircle)
			{
				pt3dCenter = pCircle->center();
			}
		}
	}

	switch (osnapMode)
	{
	case AcDb::kOsModeCen:
		snapPoints.append(pt3dCenter);
		break;

	case AcDb::kOsModeMid:
		snapPoints.append(pt3dMidLine);
		break;

	case AcDb::kOsModeEnd:
		snapPoints.append(pt3dFirstLine);
		snapPoints.append(pt3dSecondLine);
		break;

	default:
		break;
	}

	return Acad::eOk;
}
#pragma endregion