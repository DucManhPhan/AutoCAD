#pragma once
#include "stdafx.h"

#define RADIUS				10
#define COLOR_INDEX			5

class AcDbSelfCircle : public AcDbCircle
{
public:
	ACRX_DECLARE_MEMBERS(AcDbSelfCircle);

	AcDbSelfCircle();
	~AcDbSelfCircle();

	virtual Adesk::Boolean		subWorldDraw(AcGiWorldDraw* mode);
	virtual Acad::ErrorStatus	dwgInFields(AcDbDwgFiler* pFiler);
	virtual Acad::ErrorStatus	dwgOutFields(AcDbDwgFiler* pFiler);

	void						updateInforCircle(const AcGePoint3d& center, int radius = RADIUS, int color = COLOR_INDEX);

	// Commands to change properties of Balloon.
	//void						setSizeBalloon(int nRadius);
	//void						setColorBalloon(int nColor);
	void						setInnerText(tstring strText);

private:	
	/*int						m_nRadius;
	AcGePoint3d					m_pt3dCenter;
	int							m_nColor;*/
	tstring						m_strText;	
};