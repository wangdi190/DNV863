#include "StdAfx.h"
#include "BasicModel.h"


CBasicModel::CBasicModel(void)
{
	m_pd3dVB=nullptr;
	m_pd3dIB=nullptr;

	g_pTexture=NULL;
	g_pTexture2=NULL;

	recentUseTime=0;
	isInner=false;

	D3DXMatrixIdentity(&world);
	mesh=NULL;
	isDicTexture=false;
	isDicTexture2=false;

	isReceivePick=false;
	pickFlagId=0; 

	axis=D3DXVECTOR3(0,0,1);
	angle=0;
	
	ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
}


CBasicModel::~CBasicModel(void)
{
	for(auto iter=submodels.begin();iter!=submodels.end();iter++)
	{
		delete iter->second;
	}


	if (m_pd3dVB!=nullptr)
		m_pd3dVB->Release();
	if (m_pd3dIB!=nullptr)
		m_pd3dIB->Release();
	if (g_pTexture!=NULL && !isDicTexture)
		g_pTexture->Release();
	if (g_pTexture2!=NULL  && !isDicTexture2)
		g_pTexture2->Release();
}

void
CBasicModel::calWorld()
{}

//========================================================
//全体对象，静态查找函数
//========================================================
CBasicModel* 
CBasicModel::find(int rootid, int findid, CTriangleRenderer* myTri)
{

		
		auto iter=myTri->newmodels.find(rootid);
		if (iter!=myTri->newmodels.end())
		{
			return (iter->second)->find(findid);
		}

	return nullptr;
}



CBasicModel* 
CBasicModel::find(int findid)
{
	if (id==findid)
		return this;
	else
	{
		for(auto iter=submodels.begin();iter!=submodels.end();iter++)
		{
			CBasicModel* result= iter->second->find(findid);
			if (result!=nullptr)
				return result;
		}
		return nullptr;
	}
}



//------------------------------------------------
//拾取模型，返回id
//------------------------------------------------
int
CBasicModel::Pick_ModelMesh(IDirect3DDevice9* pd3dDevice, POINT ptCursor, LPD3DXMESH testmesh)
{
	int result=0;

	if (testmesh==NULL)
		return 0;

	DWORD g_dwNumIntersections = 0L;


	//计算拾取射线相关变量声明
	//POINT        ptCursor;  //鼠标位置
	D3DXMATRIX   matWorld, matView, pmatProj, m;
	D3DXVECTOR3  vPickRayOrig, vPickRayDir;
	int          iWidth, iHeight;

	//获取后台缓冲区的宽度和高度
	iWidth =myTri->para.D3DWidth;// DXUTGetBackBufferSurfaceDesc()->Width;
	iHeight =myTri->para.D3DHeight;// DXUTGetBackBufferSurfaceDesc()->Height;

	//获取当前鼠标在窗口客户区中的位置
	//GetCursorPos( &ptCursor );
	//ScreenToClient( DXUTGetHWND(), &ptCursor );


	//获取当前设备的变换矩阵
	//pd3dDevice->GetTransform( D3DTS_WORLD, &matWorld );
	matWorld=world;
	pd3dDevice->GetTransform( D3DTS_VIEW, &matView );
	pd3dDevice->GetTransform( D3DTS_PROJECTION, &pmatProj );

	//计算世界观察矩阵的逆矩阵
	D3DXMATRIX mWorldView = matWorld * matView;
	D3DXMatrixInverse( &m, NULL, &mWorldView );

	//计算拾取射线的方向与原点
	D3DXVECTOR3 vTemp;
	vTemp.x =  ((( 2.0f * ptCursor.x ) / iWidth  ) - 1 ) / pmatProj._11;
	vTemp.y = -((( 2.0f * ptCursor.y ) / iHeight ) - 1 ) / pmatProj._22;
	vTemp.z =  -1.0f; //zh注：右手坐标系，取-1

	vPickRayDir.x  = vTemp.x*m._11 + vTemp.y*m._21 + vTemp.z*m._31;
	vPickRayDir.y  = vTemp.x*m._12 + vTemp.y*m._22 + vTemp.z*m._32;
	vPickRayDir.z  = vTemp.x*m._13 + vTemp.y*m._23 + vTemp.z*m._33;

	vPickRayOrig.x = m._41;
	vPickRayOrig.y = m._42;
	vPickRayOrig.z = m._43;

	//计算被拾取到的三角形, 得到拾取到三角形的索引




	BOOL               bHit;
	LPD3DXBUFFER       pBuffer = NULL;
	D3DXINTERSECTINFO* pIntersectInfoArray;

	//D3DXIntersect( pMesh, &vPickRayOrig, &vPickRayDir, &bHit, 
	//	NULL, NULL, NULL, NULL, 
	//	&pBuffer, &g_dwNumIntersections );

	D3DXIntersect( testmesh, &vPickRayOrig, &vPickRayDir, &bHit, 
		NULL, NULL, NULL, NULL, 
		&pBuffer, &g_dwNumIntersections );
	SAFE_RELEASE( pBuffer );
	if( g_dwNumIntersections > 0 )
	{
		return id;
	}
	else
	{
		return 0;
	}
}
//------------------------------------------------
//拾取模型，返回id
//------------------------------------------------
int
CBasicModel::Pick_Model(IDirect3DDevice9* pd3dDevice, POINT ptCursor)
{
	int result=Pick_ModelMesh(pd3dDevice, ptCursor, mesh);

	if( result !=0 )
	{
		return id;
	}
	else
	{
		for (auto subiter=submodels.begin();subiter!=submodels.end();subiter++)
		{
			result=subiter->second->Pick_Model(pd3dDevice,ptCursor);
			if (result!=0)
				return result;
		}
	}

	


	return result;

}


//------------------------------------------------
//递归设置操作状态，包括子对象
//------------------------------------------------
void
CBasicModel::setOperate(int op)
{
	operate=op;
	for (auto iter=submodels.begin();iter!=submodels.end();iter++)
	{
		if (!iter->second->isInner) //非内部模型才设置
			iter->second->setOperate(op);
	}
}
//------------------------------------------------
//递归清理子对象中operate=op的项
//------------------------------------------------
void
CBasicModel::clearSub(int op)
{
	auto iter=submodels.begin();
	while (iter!=submodels.end()) //删除指定条目
	{
		if ((iter->second)->operate==op && !iter->second->isInner)  //内部模型不能删除
		{
			delete iter->second;
			iter=submodels.erase(iter);
		}
		else
		{
			iter->second->clearSub(op);
			iter++;
		}
	}



}

//------------------------------------------------
//改变颜色，重置材质
//------------------------------------------------
//void 
//CBasicModel::changeColor(DWORD color)
//{
//	ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
//	D3DXCOLOR c=D3DXCOLOR(color);
//	mtrl.Diffuse.r = mtrl.Ambient.r = c.r;//1.0f;
//	mtrl.Diffuse.g = mtrl.Ambient.g =c.g;// 0.0f;
//	mtrl.Diffuse.b = mtrl.Ambient.b =c.b;// 0.0f;
//	mtrl.Diffuse.a = mtrl.Ambient.a =c.a;// 1.0f;
//
//}