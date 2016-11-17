//+-----------------------------------------------------------------------------
//
//  CTriangleRenderer
//
//      Subclass of CRenderer that renders a single, spinning triangle
//
//------------------------------------------------------------------------------

#include "StdAfx.h"
#include <process.h> 
#include <algorithm>
//===========================================================
// 新线程处理瓦片mesh和材质载入
//===========================================================
static unsigned __stdcall
LoadTileThread(void* pArguments)
{
	CTriangleRenderer* pthis=(CTriangleRenderer*) pArguments;

	pthis->LoadTile();

	_endthreadex(   0   );  
    return   0; 
}


int cmp(const VECPAIR& x, const VECPAIR& y) {return x.second<y.second;}
//+-----------------------------------------------------------------------------
//
//  Member:
//      CTriangleRenderer ctor
//
//------------------------------------------------------------------------------
CTriangleRenderer::CTriangleRenderer() : CRenderer(), m_pd3dVB(NULL) 
{
	debuginfo=L"";
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CTriangleRenderer dtor
//
//------------------------------------------------------------------------------
CTriangleRenderer::~CTriangleRenderer()
{
    SAFE_RELEASE(m_pd3dVB);

}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CTriangleRenderer::Create
//
//  Synopsis:
//      Creates the renderer
//
//------------------------------------------------------------------------------
HRESULT 
CTriangleRenderer::Create(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter, CRenderer **ppRenderer,UINT uWidth,UINT uHeight)
{
	


    HRESULT hr = S_OK;

    CTriangleRenderer *pRenderer = new CTriangleRenderer();
    IFCOOM(pRenderer);

    IFC(pRenderer->Init(pD3D, pD3DEx, hwnd, uAdapter, uWidth,uHeight));

    *ppRenderer = pRenderer;
    pRenderer = NULL;


Cleanup:
    delete pRenderer;

    return hr;
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CTriangleRenderer::Init
//
//  Synopsis:
//      Override of CRenderer::Init that calls base to create the device and 
//      then creates the CTriangleRenderer-specific resources
//
//------------------------------------------------------------------------------
HRESULT 
CTriangleRenderer::Init(IDirect3D9 *pD3D, IDirect3D9Ex *pD3DEx, HWND hwnd, UINT uAdapter,UINT uWidth,UINT uHeight)
{

    HRESULT hr = S_OK;

	statusTile=0;
	statusModel=0;

	//models[0]=&linemodels;
	//newmodels[0]=&newlinemodels;
	//models[1]=&symbolmodels;
	//newmodels[1]=&newsymbolmodels;
	//models[2]=&polycolmodels;
	//newmodels[2]=&newpolycolmodels;
	//models[3]=&areamodels;
	//newmodels[3]=&newareamodels;
	//models[4]=&textmodels;
	//newmodels[4]=&newtextmodels;


	earthpara.isShowOverlay=true;
	earthpara.mapType=卫星;

	for (int i=0; i<sizeof(lights)/sizeof(STRUCT_Light); i++)  //初始化光源数组
	{
		ZeroMemory(&(lights[i]), sizeof(STRUCT_Light));
	
	}


	D3DXVECTOR3 POS=D3DXVECTOR3(2177.739,4094.09,4388.384);

	CCamera::Create(POS,D3DXVECTOR3(0.0f, 0.0f, 0.0f), D3DXVECTOR3(0.0f, 1.0f, 0.0f), D3DX_PI / 4, 1.0f, 1000.0f,1.0f, &camera);
 

    // Call base to create the device and render target
    IFC(CRenderer::Init(pD3D, pD3DEx, hwnd, uAdapter,uWidth,uHeight));

	IFC(m_pd3dDevice->SetTransform(D3DTS_VIEW, &camera->view));
	IFC(m_pd3dDevice->SetTransform(D3DTS_PROJECTION, &camera->projection));

    // Set up the global state
    IFC(m_pd3dDevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_CW));

	//D3DCOLOR ambientColor=D3DCOLOR_XRGB(128,128,128);
	//IFC(m_pd3dDevice->SetRenderState(D3DRS_LIGHTING, true));
	//m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, ambientColor);
	


	//初始化定义用于生成文字的设备
	D3DXCreateFont(m_pd3dDevice,20,8,0,0,0,0,0,0,0, L"Arial", &para.g_pFont2D);	
	D3DXCreateFont(m_pd3dDevice,50,20,4,0,0,0,0,0,0, L"Microsoft YaHei", &para.g_pFont3D);	

	//初始化基础材质
    ZeroMemory( &basicmtrl, sizeof(D3DMATERIAL9) );
	basicmtrl.Diffuse.r = basicmtrl.Ambient.r = 1;//1.0f;
    basicmtrl.Diffuse.g = basicmtrl.Ambient.g =1;// 0.0f;
    basicmtrl.Diffuse.b = basicmtrl.Ambient.b =1;// 0.0f;
    basicmtrl.Diffuse.a = basicmtrl.Ambient.a =1;// 1.0f;

Cleanup:
    return hr;
}

//+-----------------------------------------------------------------------------
//
//  Member:
//      CTriangleRenderer::Render
//
//  Synopsis:
//      Renders the rotating triangle
//
//------------------------------------------------------------------------------
HRESULT
CTriangleRenderer::Render()
{
    HRESULT hr = S_OK;
	//============== 光照设置 ==================
  /*  m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );

	for (int i=0; i<8; i++)
	{
		if (lights[i].isEnable)
			m_pd3dDevice->SetLight( i, &lights[i].light );
	    m_pd3dDevice->LightEnable( i, lights[i].isEnable );
	}*/



	m_pd3dDevice->SetRenderState(D3DRS_NORMALIZENORMALS, TRUE); //归一化处理法线，使模型法线不受缩放影响



	//设置基础材质
	m_pd3dDevice->SetMaterial( &basicmtrl );



	//  相机动画
	if (camera->isAni)
	{
		float progress=(float)(GetTickCount()-camera->startTick)/camera->duration;
		if (progress>=1)
		{
			progress=1;
			camera->isAni=false;
		}
		camera->aniCamera(progress);
		m_pd3dDevice->SetTransform(D3DTS_VIEW, &camera->view);
		m_pd3dDevice->SetTransform(D3DTS_PROJECTION, &camera->projection);
	}



    IFC(m_pd3dDevice->BeginScene());

	IFC(m_pd3dDevice->Clear(
        0,
        NULL,
		D3DCLEAR_TARGET| D3DCLEAR_ZBUFFER, //zh深度测试
        earthpara.background,  // NOTE: Premultiplied alpha!D3DCOLOR_ARGB(255, 255, 0, 0)
        1.0f,
        0
        ));



	//============ 绘制瓦片


	if (this->earthpara.mapType!=无)
	{
		CMapTile::SetEffect(m_pd3dDevice,this);
		if (statusTile==2) //瓦片载入完成，更新maps
		{
			UpdateMaps();
			statusTile=0;
		}
		for (auto ent=maps.begin();ent!=maps.end();ent++)//循环绘制瓦片
		{
			ent->second->Render(m_pd3dDevice);
		}
	}


	//=========== 更新模型
	if (statusModel==2) //模型载入完成，更新models
	{
		UpdateModels();
		statusModel=0;
	}


	////=========== 绘制区域
	//CPArea::SetEffect(m_pd3dDevice,this);
	//for (auto ent=areamodels.begin();ent!=areamodels.end();ent++)
	//	ent->second->Render(m_pd3dDevice);

	////=========== 绘制线路
	//CPline::SetEffect(m_pd3dDevice,this);
	//for (auto ent=linemodels.begin();ent!=linemodels.end();ent++)
	//	ent->second->Render(m_pd3dDevice);
	////=========== 绘制图元
	//CPInfoPanel::SetEffect(m_pd3dDevice,this);
	//for (auto ent=symbolmodels.begin();ent!=symbolmodels.end();ent++)
	//	ent->second->Render(m_pd3dDevice);

	////=========== 绘制文字
	//CPText::SetEffect(m_pd3dDevice,this);
	//for (auto ent=textmodels.begin();ent!=textmodels.end();ent++)
	//	ent->second->Render(m_pd3dDevice);
	////=========== 绘制几何体
	//CPPolyCol::SetEffect(m_pd3dDevice,this);
	//for (auto ent=vecPolycol.begin();ent!=vecPolycol.end();ent++)
	//{
	//	polycolmodels.find(ent->first)->second->Render(m_pd3dDevice);
	//}



	for (auto ent=vecModel.begin();ent!=vecModel.end();ent++)
	{
		models.find(ent->first)->second->Render(m_pd3dDevice);
	}




	IFC(m_pd3dDevice->EndScene());


	//m_pd3dDevice->Present(NULL,NULL,NULL,NULL); 
	//m_pd3dRTS->Release();


	//debug信息
	//RECT rect;
	//rect.left=0;
	//rect.right=200;
	//rect.top=200;
	//rect.bottom=400;
	//para.g_pFont2D->DrawText(NULL, debuginfo.c_str(), debuginfo.length(), &rect,DT_SINGLELINE|DT_NOCLIP|DT_CENTER|DT_VCENTER, 0xff00ffff);

	
Cleanup:
    return hr;
}


#pragma region 相机控制  

void CTriangleRenderer::ChangeCameraAspect(float newaspect)
{
	camera->Aspect=newaspect;
	camera->Update();

	m_pd3dDevice->SetTransform(D3DTS_VIEW, &camera->view);
	m_pd3dDevice->SetTransform(D3DTS_PROJECTION, &camera->projection);


}


void 
CTriangleRenderer::ChangeCameraPara(void* pPara,bool isAni,int duration)
{
	STRUCT_Camera* para=(STRUCT_Camera*)pPara;
	if (isAni)
	{
		memcpy(&camera->oldpara,&camera->curpara,sizeof(STRUCT_Camera));
		memcpy(&camera->newpara,para,sizeof(STRUCT_Camera));
		camera->isAni=true;
		camera->startTick=GetTickCount();
		camera->duration=duration;
	}
	else
	{
		memcpy(&camera->curpara,para,sizeof(STRUCT_Camera));
		camera->Update();
		m_pd3dDevice->SetTransform(D3DTS_VIEW, &camera->view);
		m_pd3dDevice->SetTransform(D3DTS_PROJECTION, &camera->projection);
	}
}

#pragma endregion
void 
CTriangleRenderer::ChangeLightPara(int lightNum, void* pPara)
{
	m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );

	//保存
	STRUCT_Light* ppara=(STRUCT_Light*)pPara;
	memcpy(&(lights[lightNum]),ppara,sizeof(STRUCT_Light));
	//生效
	m_pd3dDevice->SetLight( lightNum, &(lights[lightNum].light) );
    m_pd3dDevice->LightEnable( lightNum, lights[lightNum].isEnable );
}
void 
CTriangleRenderer::ChangeAmbientLight(UINT color)
{
	m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
	D3DXCOLOR c=D3DXCOLOR(color);
	m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, c);
}


#pragma region 瓦片处理相关
//************************************************************
//模式：wpf每次在begintransfer和endtransfer之间传递整套瓦片序列
//流程: 
// 1. wpf调用begintransfer, 设置buffer中瓦片operate为初始状态
// 2. wpf调用addmaptile添加瓦片，若已在buffer中，置operate为1
// 3. wpf调用endtransfer, 删除buffer中，operate不为1的项，调用新线程，初始化新加入的瓦片
// 4. 新线程结束后，将置管理器的status为已完成
// 5. wpf调用render时，检查管理器的status为已完成，则更新buffer到maps
//************************************************************
//============================================================
// 开始传输瓦片数据，失败返回0，成功返回1
//============================================================
int
CTriangleRenderer::BeginTransfer()
{
	while (statusTile!=0) 
	{
		isCancleTile=true;//return 0;  //可尝试改为提前结束运算线程
		Sleep(10);
	}

	if ((statusTile==0) || (statusTile==1)) //对所有已存在的瓦片，操作状态置初始态
	{
		for (auto ent=newmaps.begin();ent!=newmaps.end();ent++)
		{
			(ent->second)->operate=0;
		}

		return 1;
	}
	else
		return 0;


}
//===========================================================
// 结束瓦片传输，开新线程
//===========================================================
void
CTriangleRenderer::EndTransfer()
{
	//===== 清理buffer序列, 1.删除初始状的条目; 2. 已在maps序列中的置为已存在
	auto iter=newmaps.begin();
	while (iter!=newmaps.end()) //删除无效条目
	{
		if ((iter->second)->operate==0)
		{
			if (maps.find(iter->first)==maps.end()) //在显示序列中不存在的，删除创建的模型对象
				delete iter->second;
			iter=newmaps.erase(iter);
		}
		else
			iter++;
	}
	
	for (auto ent=newmaps.begin();ent!=newmaps.end();ent++) //已在maps序列中的置为已存在
	{
		auto mapsiter=maps.find(ent->first);
		if (mapsiter!=maps.end())
			(ent->second)->operate=2;
	}

	//===== 新线程处理载入
	

	HANDLE   hThread;  
    unsigned   threadID;  
  
	statusTile=1;
    hThread   =   (HANDLE)_beginthreadex(NULL, 0, &LoadTileThread, this, 0, &threadID   );   
	isCancleTile=false;
	CloseHandle(hThread);
}


int cmpBuffer(const BUFFERPAIR& x, const BUFFERPAIR& y) {return x.second>y.second;}
//===========================================================
// 更新Maps, 从作为buffer的newmaps, 更新到maps
//===========================================================
void
CTriangleRenderer::UpdateMaps()
{
	//==删除不再有效的瓦片, 在等待队列中没有的瓦片将被移除
	auto iter=maps.begin();
	while (iter!=maps.end()) 
	{
		if (newmaps.find(iter->first)==newmaps.end())
		{
			if (buffermaps.find(iter->first)==buffermaps.end()) //buffer中没有，入buffer
			{
				//buffer超1000后的处理
				if (buffermaps.size()>1000)
				{
					vector<BUFFERPAIR> vecBuffer;
					for (auto biter=buffermaps.begin();biter!=buffermaps.end();biter++)
						vecBuffer.push_back(make_pair(biter->first,(biter->second)->recentUseTime));
					sort(vecBuffer.begin(),vecBuffer.end(),cmpBuffer);
					//真删除N条
					for (int i=0;i<30;i++)
					{
						delete buffermaps.find(vecBuffer.begin()->first)->second;
						buffermaps.erase(vecBuffer.begin()->first);
						vecBuffer.erase(vecBuffer.begin());
					}
				}

				//入buffer
				buffermaps.insert(EntryMap(iter->first,iter->second));	
				iter->second->recentUseTime=GetTickCount();
			}
			iter=maps.erase(iter);
		}
		else
			iter++;
	}
	//==增加新的瓦片，在等待队列中操作为1(新增)的才增加，其它2(已存在)跳过
	for (auto ent=newmaps.begin();ent!=newmaps.end();ent++)
	{
		if ((ent->second)->operate==1)  //若为新增才添加
		{
			maps.insert(EntryMap(ent->first,ent->second));
		}
	}
}


//===============================================
// 生成瓦片mesh, 并载入材质, 被新线程调用
//===============================================
void
CTriangleRenderer::LoadTile()
{

		Concurrency::parallel_for_each(newmaps.begin(),newmaps.end(),[](pair<int,CBasicModel*> ent)
			{
				if (((ent.second)->operate==1) && ((ent.second)->status==0))  //新增且未载入的
					((CMapTile*)ent.second)->Init();
			}
			);

	////单线程载入
	//for (auto ent=newmaps.begin();ent!=newmaps.end();ent++) //遍历载入
	//{
	//	if (((ent->second)->operate==1) && ((ent->second)->status==0))  //新增且未载入的
	//		((CMapTile*)ent->second)->Init();
	//	if (isCancleTile)
	//	{
	//		statusTile=0;
	//		return;
	//	}
	//}


	statusTile=2;
}
//===============================================
// 添加瓦片
//===============================================
void CTriangleRenderer::AddMapTile(int id, int zlayer, int idxx,int idxy, bool isShowTerrain, int terrainSpan, void* pHigh)
{

	auto iter= newmaps.find(id);
	if (iter==newmaps.end()) //待载入序列中无
	{
		auto bufferiter=buffermaps.find(id);
		if (bufferiter==buffermaps.end()) //buffer中也无，完全新增
		{
			CBasicModel *tmp;
			CMapTile::Create(m_pd3dDevice,&tmp, id,zlayer,idxx,idxy,isShowTerrain,terrainSpan, pHigh,this);
			tmp->operate=1; //置操作状态
			newmaps.insert(EntryMap(id,tmp));

			tmp=NULL;
		}
		else  //buffer中找到，添加到待载入序列
		{
			newmaps.insert(EntryMap(id,bufferiter->second));
			bufferiter->second->operate=1;
			((CMapTile*)(bufferiter->second))->SetTerrain(isShowTerrain,terrainSpan,pHigh);
			buffermaps.erase(id); //从buffer中移除
		}
	}
	else
	{
		iter->second->operate=1;

		((CMapTile*)(iter->second))->SetTerrain(isShowTerrain,terrainSpan,pHigh);
	}

}




#pragma endregion

#pragma region 模型相关
//************************************************************
//模式：wpf每次在begintransfermodel和endtransfermodel之间传递整套模型序列
//流程: 
// 1. wpf调用begintransfermodel, 设置buffer中模型operate为初始状态
// 2. wpf调用addmodel添加模型，若已在buffer中，置operate为1, 若是全新的，直接生成（注：不用新线程是因为为了使模型的增删立即生效）
// 3. wpf调用endtransfermodel, 删除buffer中，operate不为1的项, 管理器的status为已完成
// 4. wpf调用render时，检查管理器的status为已完成，则更新buffer到maps
//************************************************************

//============================================================
// 开始传输模型数据，失败返回0，成功返回1, 传输的同时，生成model
//============================================================
int
CTriangleRenderer::BeginTransferModel()
{
	while (statusModel==1) 
	{
		Sleep(10);
	}

	if (statusModel==0) //对所有已存在的模型，操作状态置初始态
	{
		//for (int i=0;i<sizeof(newmodels)/sizeof(newmodels[0]);i++)
		//{
		//	for (auto iter=newmodels[i]->begin();iter!=newmodels[i]->end();iter++)
		//	{
		//		iter->second->setOperate(0); //包括子对象
		//	}
		//}
		for (auto iter=newmodels.begin();iter!=newmodels.end();iter++)
			iter->second->setOperate(0);

		return 1;
	}
	else
		return 0;


}
//===========================================================
// 结束模型传输
//===========================================================
void
CTriangleRenderer::EndTransferModel()
{

	
		//auto showmodels=models[i];  //呈现的模型集合
		//auto buffermodels=newmodels[i];   //相应为buffer的模型集合


		//===== 清理buffer序列, 1.删除在显示序列中没有的初始状的条目; 2. 已在maps序列中的置为已存在
		auto iter=newmodels.begin();
		while (iter!=newmodels.end()) //删除无效条目
		{
			if ((iter->second)->operate==0)
			{
				if (models.find(iter->first)==models.end()) //在显示序列中不存在的，删除创建的模型对象
					delete iter->second;

				iter=newmodels.erase(iter);
			}
			else
			{
				iter->second->clearSub(0); //zh注：子集进行了直接清理，可能会有冲突
				iter++;
			}
		}

		for (auto ent=newmodels.begin();ent!=newmodels.end();ent++) //已在呈现序列中的置为已存在
		{
			auto mapsiter=models.find(ent->first);
			if (mapsiter!=models.end())
			{
				(ent->second)->operate=2;
				if (mapsiter->second!=ent->second)  //若buffer中对象与呈现系列中对象不同，用buffer取代原系列中对象
				{
					delete mapsiter->second;
					mapsiter->second=ent->second;
				}
			}
		}
	
	statusModel=2; //状态置为完成，以便render更新




	//for (int i=0;i<sizeof(models)/sizeof(models[0]);i++)
	//{
	//	auto showmodels=models[i];  //呈现的模型集合
	//	auto buffermodels=newmodels[i];   //相应为buffer的模型集合


	//	//===== 清理buffer序列, 1.删除在显示序列中没有的初始状的条目; 2. 已在maps序列中的置为已存在
	//	auto iter=buffermodels->begin();
	//	while (iter!=buffermodels->end()) //删除无效条目
	//	{
	//		if ((iter->second)->operate==0)
	//		{
	//			if (showmodels->find(iter->first)==showmodels->end()) //在显示序列中不存在的，删除创建的模型对象
	//				delete iter->second;

	//			iter=buffermodels->erase(iter);
	//		}
	//		else
	//		{
	//			iter->second->clearSub(0); //zh注：子集进行了直接清理，可能会有冲突
	//			iter++;
	//		}
	//	}

	//	for (auto ent=buffermodels->begin();ent!=buffermodels->end();ent++) //已在呈现序列中的置为已存在
	//	{
	//		auto mapsiter=showmodels->find(ent->first);
	//		if (mapsiter!=showmodels->end())
	//			(ent->second)->operate=2;
	//	}
	//}
	//statusModel=2; //状态置为完成，以便render更新

	
}

//===========================================================
// 更新Models, 从作为buffer的newmodels, 更新到models
//===========================================================
void
CTriangleRenderer::UpdateModels()
{
	//for (int i=0;i<sizeof(models)/sizeof(models[0]);i++)
	//{
	//	auto showmodels=models[i];  //呈现的模型集合
	//	auto buffermodels=newmodels[i];   //相应为buffer的模型集合

	//	//==删除不再有效的模型, 1.在buffer中没有的项将被删除
	//	auto iter=showmodels->begin();
	//	while (iter!=showmodels->end()) 
	//	{
	//		auto newiter=buffermodels->find(iter->first);
	//		if (newiter==buffermodels->end())
	//		{
	//			delete iter->second;
	//			iter=showmodels->erase(iter);
	//		}
	//		else
	//			iter++;
	//	}


	//	//==增加新的模型，在buffer中操作为1(新增)的才增加，其它2(已存在)跳过
	//	for (auto iter=buffermodels->begin();iter!=buffermodels->end();iter++)
	//	{
	//		if ((iter->second)->operate==1)  //若为新增才添加
	//		{
	//			showmodels->insert(EntryMap(iter->first,iter->second));
	//		}
	//	}
	//}

	//// 在无深度测试时，增加处理polycol的排序
	//vecPolycol.clear();
	//for (auto iter=polycolmodels.begin();iter!=polycolmodels.end();iter++)
	//{
	//	vecPolycol.push_back(make_pair(iter->first,((CPPolyCol*)(iter->second))->latitude));
	//}
	//sort(vecPolycol.begin(),vecPolycol.end(),cmp);


		//auto showmodels=models[i];  //呈现的模型集合
		//auto buffermodels=newmodels[i];   //相应为buffer的模型集合

		//==删除不再有效的模型, 1.在buffer中没有的项将被删除
		auto iter=models.begin();
		while (iter!=models.end()) 
		{
			auto newiter=newmodels.find(iter->first);
			if (newiter==newmodels.end())
			{
				delete iter->second;
				iter=models.erase(iter);
			}
			else
				iter++;
		}


		//==增加新的模型，在buffer中操作为1(新增)的才增加，其它2(已存在)跳过
		for (auto iter=newmodels.begin();iter!=newmodels.end();iter++)
		{
			if ((iter->second)->operate==1)  //若为新增才添加
			{
				models.insert(EntryMap(iter->first,iter->second));
			}
		}

	// 在无深度测试时，增加处理polycol的排序
	vecModel.clear();
	for (auto iter=models.begin();iter!=models.end();iter++)
	{
		vecModel.push_back(make_pair(iter->first,iter->second->deepOrd));
	}
	sort(vecModel.begin(),vecModel.end(),cmp);

}

//-----------------------------------------------------------
//增加自定义模型
//-----------------------------------------------------------
void
CTriangleRenderer::AddCustomModel(int id, void* para, void* plocation, void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture)
{
	
		auto iter= newmodels.find(id);
		if (iter==newmodels.end())  //新对象或重建
		{
			CBasicModel *tmp;
			CPCustom::Create(m_pd3dDevice,&tmp, id,para, plocation,pvertices,pnormal,vcount,pindex,icount,puv,uvcount,ptexture ,this);
			tmp->operate=1; //置操作状态
			newmodels.insert(EntryMap(id,tmp));
			tmp=NULL;
		}
		else  //已存在，不重建
		{
				iter->second->operate=1;
		}
}
//-----------------------------------------------------------
//增加模型
//-----------------------------------------------------------
void
CTriangleRenderer::AddModel(EModelType modeltype,int id, void* para, void* pmesh, int mcount, void* ptexture, int tcount)
{
	//D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;
	
	if (modeltype==折线) // 创建折线
	{
		auto iter= newmodels.find(id);
		if (iter==newmodels.end())
		{
			CBasicModel *tmp;
			CPline::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount,this);
			tmp->operate=1; //置操作状态
			newmodels.insert(EntryMap(id,tmp));
			tmp=NULL;
		}
		else
		{
			iter->second->operate=1;
		}
	}
	else if (modeltype==图元)  //水平图元符号
	{
		auto iter= newmodels.find(id);
		if (iter==newmodels.end())
		{
			CBasicModel *tmp;
			CPSymbol::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
			tmp->operate=1; //置操作状态
			newmodels.insert(EntryMap(id,tmp));
			tmp=NULL;
		}
		else
		{
			iter->second->operate=1;
		}
	}
	else if (modeltype==几何体)  //几何体
	{
		STRUCT_PolyCol* p=(STRUCT_PolyCol*)para;
		CPPolyCol* findobj=(CPPolyCol*)CPPolyCol::find(p->rootid,id, this);

		if (findobj==nullptr)
		{
			CBasicModel *tmp;
			CPPolyCol::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
			tmp->operate=1; //置操作状态
			
			if (((CPPolyCol*)tmp)->rootid==id)  //根数据加到newmodels
				newmodels.insert(EntryMap(id,tmp));
			else //非根，加至父对象的子集中
				((CPPolyCol*)tmp)->pparent->submodels.insert(EntryMap(id,tmp));

			tmp=NULL;
		}
		else
		{
			findobj->operate=1;
		}
	}
	else if (modeltype==文字)
	{
		STRUCT_Symbol* p=(STRUCT_Symbol*)para;
		if (p->rootid==id) //独立文字
		{
			auto iter= newmodels.find(id);
			if (iter==newmodels.end())
			{
				CBasicModel *tmp;
				CPText::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
				tmp->operate=1; //置操作状态
				newmodels.insert(EntryMap(id,tmp));
				tmp=NULL;
			}
			else
			{
				iter->second->operate=1;
			}

		}
		else  //附属几何体文字
		{
			CPText* findobj=(CPText*)CPPolyCol::find(p->rootid,id,this);
			if (findobj==nullptr)
			{
				CBasicModel *tmp;
				CPText::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
				tmp->operate=1; //置操作状态

				((CPText*)tmp)->pparent->submodels.insert(EntryMap(id,tmp));

				tmp=NULL;
			}
			else
			{
				findobj->operate=1;
			}
		}



	}
	else if (modeltype==区域)
	{
		auto iter= newmodels.find(id);
		if (iter==newmodels.end())
		{
			CBasicModel *tmp;
			CPArea::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
			tmp->operate=1; //置操作状态
			newmodels.insert(EntryMap(id,tmp));
			tmp=NULL;
		}
		else
		{
			iter->second->operate=1;
		}
	}
	else if (modeltype==等值图)  //等值图与区域放在同一集合中
	{
		auto iter= newmodels.find(id);
		if (iter==newmodels.end())
		{
			CBasicModel *tmp;
			CPPanel::Create(m_pd3dDevice,&tmp, id,para, pmesh, mcount, ptexture, tcount,this);
			tmp->operate=1; //置操作状态
			newmodels.insert(EntryMap(id,tmp));
			tmp=NULL;
		}
		else
		{
			iter->second->operate=1;
		}
	}

}




#pragma endregion


//========================================================
//暂用固定枚举方式修改属性，以后有时间可以考虑实现反射
//========================================================
void 
CTriangleRenderer::ChangeProperty(EModelType modeltype,EPropertyType propertytype,int id ,int subid,void* para,int count, void* para2, int count2)
{
	if (modeltype==几何体)
	{
		CPPolyCol* obj=(CPPolyCol*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(propertytype,para,count);
	}
	if (modeltype==图元)
	{
		CPSymbol* obj=(CPSymbol*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(modeltype,propertytype,para,count);
	}
	if (modeltype==自定义模型)
	{
		CPCustom* obj=(CPCustom*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(modeltype,propertytype,para,count);
	}
	if (modeltype==折线 || modeltype==潮流)
	{
		CPline* obj=(CPline*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(modeltype, propertytype,para,count);
	}
	if (modeltype==区域)
	{
		CPArea* obj=(CPArea*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(modeltype, propertytype,para,count, para2, count2);
	}
	else if (modeltype==等值图)
	{
		auto iter=models.find(id);
		if (iter!=models.end())
		{
			((CPPanel*)iter->second)->ChangeProperty(propertytype,para,count);
		}
	}
	else if (modeltype==地图)
	{
		if (propertytype==地址)
		{
			wstring s((WCHAR*)para);
			CMapTile::mapIP=s;
		}
        else if (propertytype==路径)
		{
			wstring s((WCHAR*)para);
			CMapTile::mapPath=s;
		}
        else if (propertytype==路径2)
		{
			wstring s((WCHAR*)para);
			CMapTile::mapPath2=s;
		}
		else if (propertytype=参数)
		{
			EMapType oldtype= earthpara.mapType;
			memcpy(&earthpara,(STRUCT_EarthPara*)para,sizeof(earthpara));

			m_pd3dDevice->SetRenderState(D3DRS_ZENABLE,earthpara.isDepthStencil); //zh深度测试
			this->para.m_fSurfaceSettingsChanged=true;
		}
	}
	else if (modeltype==文字)
	{
		CPText* obj=(CPText*)CBasicModel::find(id, subid, this);
		if (obj!=nullptr)
			obj->ChangeProperty(propertytype,para,count);
            
	}

}



//------------------------------------------------
//拾取模型，返回id
//------------------------------------------------
int
CTriangleRenderer::Pick_Model(POINT ptCursor)
{
	int result=0;
	for (auto iter=models.begin();iter!=models.end();iter++)
	{
		if (iter->second->isReceivePick)
		{
			result=iter->second->Pick_Model(m_pd3dDevice, ptCursor);
			if (result!=0)
				return result;
		}
	}

	return result;
}

//------------------------------------------------
//拾取模型,  限定flag，返回id
//------------------------------------------------
int
CTriangleRenderer::Pick_Model(POINT ptCursor,int flagid)
{
	int result=0;
	for (auto iter=models.begin();iter!=models.end();iter++)
	{
		if (iter->second->isReceivePick && iter->second->pickFlagId==flagid)
		{
			result=iter->second->Pick_Model(m_pd3dDevice, ptCursor);
			if (result!=0)
				return result;
		}
	}

	return result;
}

