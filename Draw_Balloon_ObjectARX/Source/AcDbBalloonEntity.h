#pragma once
#include "stdafx.h"
#include "AcDbSelfLine.h"
#include "AcDbSelfCircle.h"

#include "dbxHeaders.h"



class AcEdInsertBalloonJig;

class AcDbBalloonEntity : public AcDbEntity
{
public:
	ACRX_DECLARE_MEMBERS(AcDbBalloonEntity);

	AcDbBalloonEntity();
	~AcDbBalloonEntity();

	std::vector<AcDbEntity*>				getEntities();
	void									setEntities(std::vector<AcDbEntity*>&& vtEntities);
	
	std::vector<AcDbObjectId>				getObjectIds();
	void									setObjectIds(std::vector<AcDbObjectId>&& vtObjectIds);

	Adesk::Boolean							subWorldDraw(AcGiWorldDraw *wd);
	void									updateInfor(const AcEdInsertBalloonJig& jig);

	void									groups();
	void									makeGroup(const AcDbObjectId& groupId);

	void									setSizeBalloon(int size);
	void									setColorBalloon(int color);
	void									setTextBalloon(tstring strText);

	virtual Acad::ErrorStatus				subGetOsnapPoints(AcDb::OsnapMode     osnapMode,
														   Adesk::GsMarker     gsSelectionMark,
														   const AcGePoint3d&  pickPoint,
														   const AcGePoint3d&  lastPoint,
														   const AcGeMatrix3d& viewXform,
														   AcGePoint3dArray&   snapPoints,
														   AcDbIntArray &   geomIds);

private:
	std::vector<AcDbEntity*>				m_vtEntities;
	std::vector<AcDbObjectId>				m_vtObjectIds;
};