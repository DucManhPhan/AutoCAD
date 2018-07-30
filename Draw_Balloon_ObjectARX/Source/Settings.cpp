#include "stdafx.h"
#include "Settings.h"


CSettings* CSettings::m_Settings = nullptr;


#pragma region Constructor && Destructor
CSettings::~CSettings() 
{
	
}


CSettings::CSettings() : m_strIsAllSelectedElement(L"N")
{
	// nothing to do.
}
#pragma endregion


#pragma region Methods
CSettings* CSettings::getInstance()
{
	if (!m_Settings)
	{
		m_Settings = new CSettings();
	}

	return m_Settings;
}


tstring CSettings::getState()
{
	return m_strIsAllSelectedElement;
}


void CSettings::setState(tstring strIsAllSeleted)
{
	m_strIsAllSelectedElement = strIsAllSeleted;
}


int CSettings::getColor()
{
	return m_nColor;
}


void CSettings::setColor(int nIndexColor)
{
	m_nColor = nIndexColor;
}


int	CSettings::getSize()
{
	return m_nSizeCircle;
}


void CSettings::setSize(int size)
{
	m_nSizeCircle = size;
}


tstring CSettings::getText()
{
	return m_strTextCircle;
}


void CSettings::setText(tstring text)
{
	m_strTextCircle = text;
}
#pragma endregion 