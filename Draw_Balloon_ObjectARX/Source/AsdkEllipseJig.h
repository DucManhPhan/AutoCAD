#pragma once

#include "stdafx.h"



class CAsdkEllipseJig : public AcEdJig
{
public:
	CAsdkEllipseJig(const AcGePoint3d&, const AcGeVector3d&);
	void doIt();
	virtual DragStatus sampler();
	virtual Adesk::Boolean update();
	virtual AcDbEntity* entity() const;

private:
	AcDbEllipse*		mpEllipse;
	AcGePoint3d			mCenterPt, mAxisPt; 
	AcGeVector3d		mMajorAxis, mNormal;
	double				mRadiusRatio;
	int					mPromptCounter;
};