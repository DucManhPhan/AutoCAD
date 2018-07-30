#pragma once
#include "stdafx.h"

#define TEXTCOLOR_INDEX		5	
#define SIZE_TEXT			10

class AcDbSelfText : public AcDbText
{
public:
	//ACRX_DECLARE_MEMBERS(AcDbSelfText);

	AcDbSelfText();
	~AcDbSelfText();

	virtual Adesk::Boolean		subWorldDraw(AcGiWorldDraw* mode);
	virtual Acad::ErrorStatus	dwgInFields(AcDbDwgFiler* pFiler);
	virtual Acad::ErrorStatus	dwgOutFields(AcDbDwgFiler* pFiler);

	void						updateInforText(const AcGePoint3d& pt, const std::basic_string<TCHAR>& text, int color = TEXTCOLOR_INDEX);

private:
	int							m_nColor;
	int							m_nHeight;
	std::basic_string<TCHAR>	m_strText;
	AcGePoint3d					m_pt3dPosition;
};