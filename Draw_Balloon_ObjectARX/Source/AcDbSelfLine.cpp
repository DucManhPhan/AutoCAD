#include "stdafx.h"
#include "AcDbSelfLine.h"



//{{AFX_ARX_MACRO
ACRX_DXF_DEFINE_MEMBERS(
	AcDbSelfLine, AcDbLine,
	AcDb::kDHL_CURRENT, AcDb::kMReleaseCurrent,
	AcDbProxyEntity::kNoOperation, ACDBSELFLINE,
	"ACDBSelfLine"
)
//}}AFX_ARX_MACRO




#pragma region Contructor & Destructor
AcDbSelfLine::AcDbSelfLine() //: m_nColor(LINECOLOR_INDEX)
{
	setColorIndex(LINECOLOR_INDEX);
}


AcDbSelfLine::~AcDbSelfLine()
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
Adesk::Boolean AcDbSelfLine::subWorldDraw(AcGiWorldDraw* mode)
{
	// check whether the place that can be able to read or not. 
	assertReadEnabled();

	AcGePoint3d pt3dStart = startPoint();
	AcGePoint3d pt3dEnd	  = endPoint();
	AcGePoint3d pnts[2]   = { pt3dStart, pt3dEnd };

	// draw the line.	
	mode->geometry().worldLine(pnts);

	return Adesk::kTrue;
}


Acad::ErrorStatus AcDbSelfLine::dwgInFields(AcDbDwgFiler* pFiler)
{
	assertWriteEnabled();

	Acad::ErrorStatus es;
	if ((es = AcDbLine::dwgInFields(pFiler)) != Acad::eOk)
	{
		return es;
	}

	// read something. 
	/*pFiler->readInt32(&m_nColor);
	pFiler->readPoint3d(&m_pt3dFirst);
	pFiler->readPoint3d(&m_pt3dSecond);*/

	return pFiler->filerStatus();
}


Acad::ErrorStatus AcDbSelfLine::dwgOutFields(AcDbDwgFiler* pFiler)
{
	assertReadEnabled();

	// Call dwgOutFields from AcDbCircle
	Acad::ErrorStatus es;
	if ((es = AcDbLine::dwgOutFields(pFiler)) != Acad::eOk) 
	{
		return es;
	}

	// write something to db.
	/*pFiler->writeInt32(m_nColor);
	pFiler->writePoint3d(m_pt3dFirst);
	pFiler->writePoint3d(m_pt3dSecond);*/
	
	return pFiler->filerStatus();
}


void AcDbSelfLine::updateInforLine(const AcGePoint3d& pt1, const AcGePoint3d& pt2, int color)
{
	/*m_pt3dFirst		= pt1; 
	m_pt3dSecond	= pt2;
	m_nColor		= color;*/

	setStartPoint(pt1);
	setEndPoint(pt2);
	setColorIndex(color);
}


//void AcDbSelfLine::setColorLine(int nColor)
//{
//	setColorIndex(nColor);
//}
#pragma endregion