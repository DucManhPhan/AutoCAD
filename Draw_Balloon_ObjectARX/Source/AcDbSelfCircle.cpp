#include "stdafx.h"
#include "AcDbSelfCircle.h"


//{{AFX_ARX_MACRO
ACRX_DXF_DEFINE_MEMBERS(
	AcDbSelfCircle, AcDbCircle,
	AcDb::kDHL_CURRENT, AcDb::kMReleaseCurrent,
	AcDbProxyEntity::kNoOperation, ACDBSELFCIRCLE,
	"ACDBSelfCircle"
)
//}}AFX_ARX_MACRO


#pragma region Constructor && Destructor
AcDbSelfCircle::AcDbSelfCircle() : m_strText(L"A") //, m_nColor(COLOR_INDEX)
{	
	setColorIndex(COLOR_INDEX);
}


AcDbSelfCircle::~AcDbSelfCircle()
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
Adesk::Boolean AcDbSelfCircle::subWorldDraw(AcGiWorldDraw* mode)
{
	assertReadEnabled();

	//AcGePoint3d pt3dCenter = center();
	//int radius = this->radius();

	// draw the Circle
	mode->geometry().circle(center(), this->radius(), AcGeVector3d(0, 0, 1));

	// set the color to m_nColor
	//mode->subEntityTraits().setColor(m_nColor);

	// draw text to show its entity.
	mode->geometry().text(center(), AcGeVector3d(0, 0, 1), AcGeVector3d(1, 0, 0), this->radius() / 10, 1.0, 0.0, m_strText.c_str());
	
	return Adesk::kTrue;
}


Acad::ErrorStatus AcDbSelfCircle::dwgInFields(AcDbDwgFiler* pFiler)
{
	assertWriteEnabled();
	Acad::ErrorStatus es;

	// Call dwgInFields from AcDbCircle
	if ((es = AcDbCircle::dwgInFields(pFiler)) != Acad::eOk) {
		return es;
	}

	// read something.
	ACHAR* strText;

	//pFiler->readInt32(&m_nColor);
	//pFiler->readInt32(&m_nRadius);
	//pFiler->readPoint3d(&m_pt3dCenter);
	if (pFiler->readString(&strText) != Acad::eOk)
	{
		return Acad::eInvalidInput;
	}

	m_strText = strText;

	return pFiler->filerStatus();
}


Acad::ErrorStatus AcDbSelfCircle::dwgOutFields(AcDbDwgFiler* pFiler)
{
	assertReadEnabled();
	Acad::ErrorStatus es;

	// Call dwgOutFields from AcDbCircle
	if ((es = AcDbCircle::dwgOutFields(pFiler)) != Acad::eOk) {
		return es;
	}

	// write something.
	//pFiler->writeInt32(m_nColor);
	//pFiler->writeInt32(m_nRadius);
	//pFiler->writePoint3d(m_pt3dCenter);
	if (pFiler->writeString(m_strText.c_str()) != Acad::eOk)
	{
		return Acad::eNotImplementedYet;
	}

	return pFiler->filerStatus();
}


void AcDbSelfCircle::updateInforCircle(const AcGePoint3d& center, int radius, int color)
{
	setCenter(center);
	setRadius(radius);
	setColorIndex(color);
}


//void AcDbSelfCircle::setSizeBalloon(int nRadius)
//{
//	setRadius(nRadius);
//}


//void AcDbSelfCircle::setColorBalloon(int nColor)
//{
//	setColorIndex(nColor);
//}


void AcDbSelfCircle::setInnerText(tstring strText)
{
	m_strText = strText;
}
#pragma endregion