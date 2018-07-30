#pragma once
#include "stdafx.h"


class CSettings
{
public:
	~CSettings();
	static CSettings*		getInstance();

	tstring					getState();
	void					setState(tstring strIsAllSeleted);

	int						getColor();
	void					setColor(int nIndexColor);

	int						getSize();
	void					setSize(int size);

	tstring					getText();
	void					setText(tstring text);

private:
	tstring					m_strIsAllSelectedElement;
	int						m_nColor;
	int						m_nSizeCircle;
	tstring					m_strTextCircle;

	static CSettings*		m_Settings;

private:
	CSettings();	
};