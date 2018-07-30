#include "stdafx.h"
#include "AsdkEllipseJig.h"


CAsdkEllipseJig::CAsdkEllipseJig(const AcGePoint3d& pt, const AcGeVector3d& normal)
	: mCenterPt(pt)
	, mNormal(normal)
	, mRadiusRatio(0.00001)
	, mPromptCounter(0)
{
	// nothing to do.
}


void CAsdkEllipseJig::doIt()
{
	mpEllipse = new AcDbEllipse();
	setDispPrompt(_T("\nEllipse major axis: "));
	AcEdJig::DragStatus stat = drag();

	// get the ellipse's radius ratio.
	++mPromptCounter;
	setDispPrompt(_T("Ellipse minor axis: "));
	stat = drag();

	// add the ellipse to the database's current space.
	append();
}


AcEdJig::DragStatus CAsdkEllipseJig::sampler()
{
	DragStatus stat;
	setUserInputControls((UserInputControls)
		(AcEdJig::kAccept3dCoordinates
			| AcEdJig::kNoNegativeResponseAccepted
			| AcEdJig::kNoZeroResponseAccepted));

	if (mPromptCounter == 0)
	{
		static AcGePoint3d axisPointTemp;
		stat = acquirePoint(mAxisPt, mCenterPt);

		if (axisPointTemp != mAxisPt)
		{
			axisPointTemp = mAxisPt;
		}
		else if (stat == AcEdJig::kNormal)
		{
			return AcEdJig::kNoChange;
		}
	}
	else if (mPromptCounter == 1)
	{
		static double radiusRatioTemp = -1;
		stat = acquireDist(mRadiusRatio, mCenterPt);
		
		if (radiusRatioTemp != mRadiusRatio)
		{
			radiusRatioTemp = mRadiusRatio;
		}
		else if (stat == AcEdJig::kNormal)
		{
			return AcEdJig::kNoChange;
		}
	}

	return stat;
}


Adesk::Boolean CAsdkEllipseJig::update()
{
	switch (mPromptCounter)
	{
	case 0: 
		mMajorAxis = mAxisPt - mCenterPt;
		break;

	case 1:
		mRadiusRatio /= mMajorAxis.length();
		break;

	default:
		break;
	}

	mpEllipse->set(mCenterPt, mNormal, mMajorAxis, mRadiusRatio);

	return Adesk::kTrue;
}


AcDbEntity* CAsdkEllipseJig::entity() const
{
	return mpEllipse;
}


