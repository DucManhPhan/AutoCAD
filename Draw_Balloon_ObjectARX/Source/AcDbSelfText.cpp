#include "stdafx.h"
#include "AcDbSelfText.h"


#pragma region Contructor & Destructor
AcDbSelfText::AcDbSelfText() : m_nColor(TEXTCOLOR_INDEX)
{
	setColorIndex(m_nColor);	
}


AcDbSelfText::~AcDbSelfText()
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
Adesk::Boolean AcDbSelfText::subWorldDraw(AcGiWorldDraw* mode)
{
	// do something.

	return Adesk::kTrue;
}


Acad::ErrorStatus AcDbSelfText::dwgInFields(AcDbDwgFiler* pFiler)
{
	assertReadEnabled();

	Acad::ErrorStatus es;
	if ((es = AcDbText::dwgInFields(pFiler)) != Acad::eOk)
	{
		return es;
	}	

	// read something.
	pFiler->readInt32(&m_nColor);

	ACHAR* strTemp;
	pFiler->readString(&strTemp);
	m_strText.assign(strTemp);

	pFiler->readPoint3d(&m_pt3dPosition);

	return pFiler->filerStatus();
}


Acad::ErrorStatus AcDbSelfText::dwgOutFields(AcDbDwgFiler* pFiler)
{
	assertWriteEnabled();

	Acad::ErrorStatus es; 
	if ((es = AcDbText::dwgOutFields(pFiler)) != Acad::eOk)
	{
		return es;
	}

	// write something.
	pFiler->writeInt32(m_nColor);
	pFiler->writeString(m_strText.c_str());
	pFiler->writePoint3d(m_pt3dPosition);

	return pFiler->filerStatus();
}


void AcDbSelfText::updateInforText(const AcGePoint3d& pt, const std::basic_string<TCHAR>& text, int color)
{
	m_pt3dPosition	= pt;
	m_nColor		= color;
	m_strText		= text;
}
#pragma endregion