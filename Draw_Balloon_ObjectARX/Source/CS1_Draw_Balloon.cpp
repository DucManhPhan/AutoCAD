// CS1_Draw_Balloon.cpp : Defines the initialization routines for the DLL.
//
#include "stdafx.h"
#include "stdarx.h"
#include "AsdkEllipseJig.h"
#include "AcEdInsertBalloonJig.h"
#include "Settings.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define BUFFER_SIZE			2
#define BUFFER_TEXT			216


#pragma region Create Command
#pragma region Update Properties
void getSelectedEntities(std::vector<AcDbEntity*>& vtEntities)
{
	// get all selected entities. 
	ads_name ss;
	if (acedSSGet(nullptr, nullptr, nullptr, nullptr, ss) != RTNORM)
	{
		return;
	}

	// get the size of the selected entities. 
	int size = 0;
	if ((acedSSLength(ss, &size) != RTNORM) || size == 0)
	{
		acedSSFree(ss);
		return;
	}

	// get all entities that satisfy some properties.
	ads_name ent;
	AcDbObjectId id = AcDbObjectId::kNull;
	AcDbEntity* pEntity = nullptr;

	for (int i = 0; i < size; ++i)
	{
		if (acedSSName(ss, i, ent) != RTNORM)
		{
			continue;
		}

		if (acdbGetObjectId(id, ent) != Acad::eOk)
		{
			continue;
		}

		if (acdbOpenAcDbEntity(pEntity, id, AcDb::kForWrite) != Acad::eOk)
		{
			continue;
		}

		if (pEntity->isKindOf(AcDbBalloonEntity::desc()))
		{
			vtEntities.push_back(pEntity);
		}
	}
}


void getAllBalloonEntities(std::vector<AcDbEntity*>& vtEntities)
{
	AcDbDatabase* pDb = acdbHostApplicationServices()->workingDatabase();
	if (!pDb)
	{
		return;
	}

	// get object Block Table.
	AcDbBlockTable* pBlockTable = nullptr;
	if (pDb->getBlockTable(pBlockTable, AcDb::kForRead) != Acad::eOk)
	{
		return;
	}

	// get object BlockTable record. 
	AcDbBlockTableRecord* pRecord = nullptr;
	if (pBlockTable != nullptr)
	{
		if (pBlockTable->getAt(ACDB_MODEL_SPACE, pRecord, AcDb::kForWrite) != Acad::eOk)
		{
			return;
		}

		// iterate all of records in Block Table.
		AcDbBlockTableRecordIterator* pRecordIterator = nullptr;
		if (pRecord->newIterator(pRecordIterator) != Acad::eOk)
		{
			return;
		}

		for (; !pRecordIterator->done(); pRecordIterator->step())
		{
			AcDbEntity* pEntity = nullptr;
			if (pRecordIterator->getEntity(pEntity, AcDb::kForWrite) == Acad::eOk)
			{
				if (pEntity->isKindOf(AcDbBalloonEntity::desc()))
				{
					vtEntities.push_back(pEntity);
				}
			}
		}

		// close the record, blocktable, objectEntity.
		pBlockTable->close();
		pRecord->close();
	}
}


void closeEnities(std::vector<AcDbEntity*>& vtEntities)
{
	if (vtEntities.empty())
	{
		return;
	}

	/*for_each(vtEntities.begin()
		, vtEntities.end()
		, [](AcDbEntity* tmp) {
		tmp->close();
	});*/

	int size = vtEntities.size();
	for (int i = 0; i < size; ++i)
	{
		if (vtEntities[i])
		{
			vtEntities[i]->close();
		}
	}

}


void updateEnitties(ModeCommand mode)
{
	// get state that is all elements or some selected elements.
	CSettings* pSetting = CSettings::getInstance();
	if (!pSetting)
	{
		return;
	}

	std::vector<AcDbEntity*> vtEntities;

	pSetting->getState() == L"Y" ? getAllBalloonEntities(vtEntities) : getSelectedEntities(vtEntities);
	if (vtEntities.empty())
	{
		return;
	}


	// apply properties according to mode of commands.
	int size = vtEntities.size();
	switch (mode)
	{
	case Size:		
	{
		int sizeBalloon = pSetting->getSize();
		for (int i = 0; i < size; ++i)
		{
			AcDbBalloonEntity* pEntity = AcDbBalloonEntity::cast(vtEntities[i]);
			if (!pEntity)
			{
				return;
			}

			pEntity->setSizeBalloon(sizeBalloon);
		}
		break;
	}		
		
	case Color:
	{
		int colorBalloon = pSetting->getColor();
		for (int i = 0; i < size; ++i)
		{
			AcDbBalloonEntity* pEntity = AcDbBalloonEntity::cast(vtEntities[i]);
			if (!pEntity)
			{
				return;
			}

			pEntity->setColorBalloon(colorBalloon);
		}
		break;
	}		

	case Text:
	{
		tstring textBalloon = pSetting->getText();
		for (int i = 0; i < size; ++i)
		{
			AcDbBalloonEntity* pEntity = AcDbBalloonEntity::cast(vtEntities[i]);
			if (!pEntity)
			{
				return;
			}

			pEntity->setTextBalloon(textBalloon);
		}
		break;
	}		

	default:
		break;
	}

	// close all entities and redraw entities in AutoCAD.
	closeEnities(vtEntities);
}
#pragma endregion


#pragma region Create Balloon
void createBalloonCommand()
{
	// get the first point.
	AcGePoint3d tempPt;
	struct resbuf rbFrom, rbTo;
	acedGetPoint(NULL, _T("\nFirst point of the line: "),
		asDblArray(tempPt));
	
	// Conversion UCS to WCS.
	rbFrom.restype		= RTSHORT;
	rbFrom.resval.rint	= 1;		// from UCS.
	rbTo.restype		= RTSHORT;
	rbTo.resval.rint	= 0;		// from WCS.

	acedTrans(asDblArray(tempPt), &rbFrom, &rbTo,
		Adesk::kFalse, asDblArray(tempPt));

	// make the balloon.
	AcEdInsertBalloonJig* pBalloon = new AcEdInsertBalloonJig(tempPt);
	if (!pBalloon)
	{
		return;
	}

	// create some entities. 
	AcDbSelfLine*	pLine	= new AcDbSelfLine();
	if (!pLine)
	{
		return;
	}

	AcDbSelfCircle* pCircle = new AcDbSelfCircle();
	if (!pCircle)
	{
		return;
	}

	std::vector<AcDbEntity*> vtEntites;
	vtEntites.push_back(pLine);
	vtEntites.push_back(pCircle);

	AcDbBalloonEntity* pCustomEntity = pBalloon->getCustomEntity();
	pCustomEntity->setEntities(std::move(vtEntites));

	// create Balloon.
	pBalloon->makeBalloon();
	
	// delete balloon.
	if (pBalloon)
	{
		delete pBalloon;
		pBalloon = nullptr;
	}
}
#pragma endregion


#pragma region Change Size of Balloon
void changeSizeBalloon()
{
	// get instance for setting.
	CSettings* setting = CSettings::getInstance();
	if (!setting)
	{
		return;
	}

	// get size of balloon.
	int size = 0;
	if (acedGetInt(L"\nThe size of balloon is: ", &size) != RTNORM && size <= 0)
	{
		return;
	}	

	setting->setSize(size);	

	// get the state for all elements. 
	ACHAR result[BUFFER_SIZE];
	if (acedGetString(Adesk::kFalse, L"\nDo you want to apply the following properties for all of elements [Y/N]: ", result, BUFFER_SIZE) != RTNORM)
	{
		return;
	}

	tstring strState = wcsncmp(result, L"Y", BUFFER_SIZE) ? L"N" : L"Y";
	setting->setState(strState);


	// update for elements. 
	updateEnitties(ModeCommand::Size);
}
#pragma endregion


#pragma region Change Color of Balloon
void changeColorBalloon()
{
	// get instance for setting.
	CSettings* setting = CSettings::getInstance();
	if (!setting)
	{
		return;
	}

	// get size of balloon.
	int color = 0;
	if (acedGetInt(L"\nThe color of balloon is: ", &color) != RTNORM && color <= 0)
	{
		return;
	}

	setting->setColor(color);


	// get the state for all elements. 
	ACHAR result[BUFFER_SIZE];
	if (acedGetString(Adesk::kFalse, L"\nDo you want to apply the following properties for all of elements [Y/N]: ", result, BUFFER_SIZE) != RTNORM)
	{
		return;
	}

	tstring strState = wcsncmp(result, L"Y", BUFFER_SIZE) ? L"N" : L"Y";
	setting->setState(strState);


	// update elements
	updateEnitties(ModeCommand::Color);
}
#pragma endregion 


#pragma region Change Text of Balloon
void changeTextBalloon()
{
	// get instance for setting.
	CSettings* setting = CSettings::getInstance();
	if (!setting)
	{
		return;
	}

	// get text for balloon.
	ACHAR text[BUFFER_TEXT];
	if (acedGetString(Adesk::kFalse, L"\nEnter your text for balloon: ", text, BUFFER_TEXT) != RTNORM)
	{
		return;
	}

	setting->setText(text);

	// get the state for all elements. 
	ACHAR result[BUFFER_SIZE];
	if (acedGetString(Adesk::kFalse, L"\nDo you want to apply the following properties for all of elements [Y/N]: ", result, BUFFER_SIZE) != RTNORM)
	{
		return;
	}

	tstring strState = wcsncmp(result, L"Y", BUFFER_SIZE) ? L"N" : L"Y";
	setting->setState(strState);


	// update elements
	updateEnitties(ModeCommand::Text);
}
#pragma endregion
#pragma endregion


#pragma region Load Command & Unload Command
void loadCommands()
{
	acedRegCmds->addCommand(_T("DRAW_BALLOON"), _T("InsertBalloon"), _T("InsertBalloon"), ACRX_CMD_MODAL, &createBalloonCommand);
	acedRegCmds->addCommand(_T("CHANGE_SIZE_BALLOON"), _T("ChangeSize"), _T("ChangeSize"), ACRX_CMD_MODAL, &changeSizeBalloon);
	acedRegCmds->addCommand(_T("CHANGE_COLOR_BALLOON"), _T("ChangeColor"), _T("ChangeColor"), ACRX_CMD_MODAL, &changeColorBalloon);
	acedRegCmds->addCommand(_T("CHANGE_TEXT_BALLOON"), _T("ChangeText"), _T("ChangeText"), ACRX_CMD_MODAL, &changeTextBalloon);
}


void unloadApp()
{
	acedRegCmds->removeGroup(_T("DRAW_BALLOON"));
	acedRegCmds->removeGroup(_T("CHANGE_SIZE_BALLOON"));
	acedRegCmds->removeGroup(_T("CHANGE_COLOR_BALLOON"));
	acedRegCmds->removeGroup(_T("CHANGE_TEXT_BALLOON"));
}
#pragma endregion


#pragma region Entry Point
extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode msg, void* appId)
{
	switch (msg) {
	case AcRx::kInitAppMsg:
		acrxUnlockApplication(appId);
		acrxRegisterAppMDIAware(appId);
		acutPrintf(_T("\nMinimum ObjectARX application loaded!"));

		// register custom entity.
		AcDbSelfLine::rxInit();
		AcDbSelfCircle::rxInit();
		AcDbBalloonEntity::rxInit();
		acrxBuildClassHierarchy();
		
		loadCommands();

		break;
	case AcRx::kUnloadAppMsg:
		acutPrintf(_T("\nMinimum ObjectARX application unloaded!"));

		// delete custom entity.
		deleteAcRxClass(AcDbBalloonEntity::desc());
		deleteAcRxClass(AcDbSelfLine::desc());
		deleteAcRxClass(AcDbSelfCircle::desc());

		unloadApp();
			break;
	}

	return AcRx::kRetOK;
}
#pragma endregion