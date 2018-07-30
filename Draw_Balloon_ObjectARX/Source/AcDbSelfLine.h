#pragma once
#include "stdafx.h"

#define LINECOLOR_INDEX		4
#define VERSION_SELFTEXT	1


class AcDbSelfLine : public AcDbLine
{
public:
	ACRX_DECLARE_MEMBERS(AcDbSelfLine);

	AcDbSelfLine();
	~AcDbSelfLine();

	virtual Adesk::Boolean		subWorldDraw(AcGiWorldDraw* mode);
	virtual Acad::ErrorStatus	dwgInFields(AcDbDwgFiler* pFiler);
	virtual Acad::ErrorStatus	dwgOutFields(AcDbDwgFiler* pFiler);

	void						updateInforLine(const AcGePoint3d& pt1, const AcGePoint3d& pt2, int color = LINECOLOR_INDEX);
	//void						setColorLine(int nColor);

private:
	/*int							m_nColor;
	AcGePoint3d					m_pt3dFirst;
	AcGePoint3d					m_pt3dSecond;*/
};
