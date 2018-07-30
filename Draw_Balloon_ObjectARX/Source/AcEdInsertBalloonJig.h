#pragma once
#include "stdafx.h"
#include "AcDbBalloonEntity.h"


#define RADIUS 10

enum ModeCommand {
	Size = 0, 
	Color, 
	Text
};



class AcEdInsertBalloonJig : public AcEdJig
{
public:
	AcEdInsertBalloonJig(const AcGePoint3d& pt1);
	AcEdInsertBalloonJig(const AcGePoint3d& pt1, int radius);

	~AcEdInsertBalloonJig();

	void						makeBalloon();

	virtual DragStatus			sampler();
	virtual Adesk::Boolean		update();
	virtual AcDbEntity*			entity() const;	

	AcDbObjectId				append(AcDbDatabase* pDb = nullptr, const ACHAR* pDbSpace = ACDB_MODEL_SPACE);
	void						pushEntityToDB(AcDbDatabase* pDb, const ACHAR* pDbSpace, AcDbEntity* pEntity);

	AcGePoint3d					getFirstPointLine() const { return m_pt3dFirstOfLine; }
	AcGePoint3d					getSecondPointLine() const { return m_pt3dSecondOfLine; }
	AcGePoint3d					getCenterPoint() const { return m_pt3dCenter; }

	AcDbBalloonEntity*			getCustomEntity();

private:
	AcDbBalloonEntity*			m_pCustomEntity;

	AcGePoint3d					m_pt3dFirstOfLine;	
	AcGePoint3d					m_pt3dSecondOfLine;
	AcGePoint3d					m_pt3dCenter;
	int							m_nRadius;	
};