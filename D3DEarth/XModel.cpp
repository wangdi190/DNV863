#include "StdAfx.h"
#include "XModel.h"


CXModel::CXModel(void)
{
	g_pMesh          = NULL;  //网格模型对象
	g_pMeshMaterials = NULL;  //网格模型材质
	g_pMeshTextures  = NULL;  //网格模型纹理
	g_dwNumMaterials = 0L;    //网格模型材质数量

	//mesh=NULL;
	g_pTexture=NULL;
}


CXModel::~CXModel(void)
{
}


//-----------------------------------------------------------------------------
// Desc: 创建场景图形，从custom类型
//-----------------------------------------------------------------------------
void CXModel::InitCustom(void* pvertices,void* pnormal, int vcount, void* pindex, int icount, void* puv, int uvcount, void* ptexture)
{
	this->isCustom=true;

	g_dwNumMaterials=1;
	//===== 材质
	g_pMeshMaterials =NULL;// new D3DMATERIAL9[1];
	//ZeroMemory( &g_pMeshMaterials[0], sizeof(D3DMATERIAL9) );
	//

	//g_pMeshMaterials[0].Diffuse.r = g_pMeshMaterials[0].Ambient.r = 0.0f;
 //   g_pMeshMaterials[0].Diffuse.g = g_pMeshMaterials[0].Ambient.g = 0.0f;
 //   g_pMeshMaterials[0].Diffuse.b = g_pMeshMaterials[0].Ambient.b = 0.0f;
 //   g_pMeshMaterials[0].Diffuse.a = g_pMeshMaterials[0].Ambient.a = 255.0f;
	
	//===== 纹理
	if (ptexture!=nullptr)
	{
		g_pMeshTextures  = new LPDIRECT3DTEXTURE9[1];
        wstring file((WCHAR*)ptexture);
		D3DXCreateTextureFromFile(CGlobal::pCurRender->m_pd3dDevice,file.c_str(),&g_pMeshTextures[0]);
	}
	else
	{
		g_pMeshTextures  = new LPDIRECT3DTEXTURE9[1];
		g_pMeshTextures[0]=nullptr;
	}

	//===== mesh

	
	D3DXVECTOR3* pv=(D3DXVECTOR3*)pvertices;
	D3DXVECTOR3* pn=(D3DXVECTOR3*)pnormal;
	D3DXVECTOR2* uv;
	if (puv!=nullptr)
		uv=(D3DXVECTOR2*)puv;

	CUSTOMVERTEX_T* vertices(nullptr);
	vertices=new CUSTOMVERTEX_T[vcount];

	for (int i=0; i<vcount; i++)
	{
		vertices[i].position=D3DXVECTOR3(pv[i].x,pv[i].y,pv[i].z);
		vertices[i].normal=D3DXVECTOR3(pn[i].x,pn[i].y,pn[i].z);
		D3DXVec3Normalize(&vertices[i].normal,&vertices[i].normal);
		if (puv!=nullptr)
		{
			vertices[i].u=uv[i].x;
			vertices[i].v=uv[i].y;
		}
	}

	WORD* pi=(WORD*)pindex;
	WORD* idxes(nullptr);
	idxes=new WORD[icount];
	memcpy(idxes,pi,sizeof(pi[0])*icount); //复制索引 

	int NumVertices=vcount; //顶点数
	int PrimCount=icount/3; //三角数


	//==== 建立mesh
	if (g_pMesh!=NULL)
		g_pMesh->Release();

	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_T, CGlobal::pCurRender->m_pd3dDevice, &g_pMesh);
	
	CUSTOMVERTEX_T* v = 0;
	g_pMesh->LockVertexBuffer(0, (void**)&v);
	for (int i=0;i<NumVertices;i++)
		v[i]=vertices[i];
	g_pMesh->UnlockVertexBuffer();

 
	WORD* idx = 0;
	g_pMesh->LockIndexBuffer(0, (void**)&idx);
	for (int i=0;i<icount;i++)
	{
		idx[i]=idxes[i];
	}
	g_pMesh->UnlockIndexBuffer();
	//优化
	vector<DWORD> adjacencyBuffer(g_pMesh->GetNumFaces() * 3);
	g_pMesh->GenerateAdjacency(0.0f, &adjacencyBuffer[0]);
	g_pMesh->OptimizeInplace(D3DXMESHOPT_ATTRSORT|D3DXMESHOPT_COMPACT|D3DXMESHOPT_VERTEXCACHE, &adjacencyBuffer[0],0, 0, 0);




	Cleanup:
	delete vertices;
	delete idxes;



//	//===== 材质
//	ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
//	mtrl.Diffuse.r = mtrl.Ambient.r = 1.0f;
//    mtrl.Diffuse.g = mtrl.Ambient.g = 0.0f;
//    mtrl.Diffuse.b = mtrl.Ambient.b = 0.0f;
//    mtrl.Diffuse.a = mtrl.Ambient.a =255.0f;	
//	//===== 纹理
//        wstring file((WCHAR*)ptexture);
//		D3DXCreateTextureFromFile(CGlobal::pCurRender->m_pd3dDevice,file.c_str(),&g_pTexture);
//	//========mesh
//
//D3DXVECTOR3* pv=(D3DXVECTOR3*)pvertices;
//	D3DXVECTOR3* pn=(D3DXVECTOR3*)pnormal;
//	D3DXVECTOR2* uv;
//	if (puv!=nullptr)
//		uv=(D3DXVECTOR2*)puv;
//
//	CUSTOMVERTEX_T* vertices(nullptr);
//	vertices=new CUSTOMVERTEX_T[vcount];
//
//	for (int i=0; i<vcount; i++)
//	{
//		vertices[i].position=D3DXVECTOR3(pv[i].x,-pv[i].y,pv[i].z);
//		vertices[i].normal=D3DXVECTOR3(pn[i].x,pn[i].y,pn[i].z);
//		if (puv!=nullptr)
//		{
//			vertices[i].u=uv[i].x;
//			vertices[i].v=uv[i].y;
//		}
//	}
//
//	WORD* pi=(WORD*)pindex;
//	WORD* idxes(nullptr);
//	idxes=new WORD[icount];
//	memcpy(idxes,pi,sizeof(pi[0])*icount); //复制索引 
//
//	int NumVertices=vcount; //顶点数
//	int PrimCount=icount/3; //三角数
//
//
//	// 建立mesh
//	if (mesh!=NULL)
//		mesh->Release();
//
//	D3DXCreateMeshFVF(PrimCount,NumVertices,D3DXMESH_DYNAMIC, D3DFVF_CUSTOMVERTEX_T,CGlobal::pCurRender->m_pd3dDevice, &mesh);
//	
//	CUSTOMVERTEX_T* v = 0;
//	mesh->LockVertexBuffer(0, (void**)&v);
//	for (int i=0;i<NumVertices;i++)
//		v[i]=vertices[i];
//	mesh->UnlockVertexBuffer();
//
// 
//	WORD* idx = 0;
//	mesh->LockIndexBuffer(0, (void**)&idx);
//	for (int i=0;i<icount;i++)
//	{
//		idx[i]=idxes[i];
//	}
//	mesh->UnlockIndexBuffer();
//	//优化
//	vector<DWORD> adjacencyBuffer(mesh->GetNumFaces() * 3);
//	mesh->GenerateAdjacency(0.0f, &adjacencyBuffer[0]);
//	mesh->OptimizeInplace(D3DXMESHOPT_ATTRSORT|D3DXMESHOPT_COMPACT|D3DXMESHOPT_VERTEXCACHE, &adjacencyBuffer[0],0, 0, 0);
//
//
//
//
//	Cleanup:
//	delete vertices;
//	delete idxes;

}


//-----------------------------------------------------------------------------
// Desc: 创建场景图形，从.x文件
//-----------------------------------------------------------------------------
HRESULT
CXModel::InitGeometry()
{
	this->isCustom=false;
    LPD3DXBUFFER pD3DXMtrlBuffer;  //存储网格模型材质的缓冲区对象

    //从磁盘文件加载网格模型

    if( FAILED( D3DXLoadMeshFromX( xfile.c_str(), D3DXMESH_DYNAMIC, 
                                   CGlobal::pCurRender->m_pd3dDevice, NULL, 
                                   &pD3DXMtrlBuffer, NULL, &g_dwNumMaterials, 
                                   &g_pMesh ) ) )
    {
        MessageBox(NULL, L"未能装载模型文件", L"Mesh", MB_OK);
        return E_FAIL;
    }

    //从网格模型中提取材质属性和纹理文件名 
    D3DXMATERIAL* d3dxMaterials = (D3DXMATERIAL*)pD3DXMtrlBuffer->GetBufferPointer();
    g_pMeshMaterials = new D3DMATERIAL9[g_dwNumMaterials];

    if( g_pMeshMaterials == NULL )
        return E_OUTOFMEMORY;

    g_pMeshTextures  = new LPDIRECT3DTEXTURE9[g_dwNumMaterials];
    if( g_pMeshTextures == NULL )
        return E_OUTOFMEMORY;

	//逐块提取网格模型材质属性和纹理文件名
    for( DWORD i=0; i<g_dwNumMaterials; i++ )
    {
        //材料属性
        g_pMeshMaterials[i] = d3dxMaterials[i].MatD3D;
		//设置模型材料的环境光反射系数, 因为模型材料本身没有设置环境光反射系数
        g_pMeshMaterials[i].Ambient = g_pMeshMaterials[i].Diffuse;

        g_pMeshTextures[i] = NULL;
        if( d3dxMaterials[i].pTextureFilename != NULL && 
            strlen(d3dxMaterials[i].pTextureFilename) > 0 )
        {
			//获取纹理文件名
			WCHAR filename[256];
			RemovePathFromFileName(d3dxMaterials[i].pTextureFilename, filename);
			
            //创建纹理
            if( FAILED( D3DXCreateTextureFromFile( CGlobal::pCurRender->m_pd3dDevice, filename, &g_pMeshTextures[i] ) ) )
            {
                MessageBox(NULL, L"Could not find texture file", L"Mesh", MB_OK);
            }
        }
    }

	//释放在加载模型文件时创建的保存模型材质和纹理数据的缓冲区对象
    pD3DXMtrlBuffer->Release();  

    return S_OK;
}

//-----------------------------------------------------------------------------
// Desc: 从绝对路径中提取纹理文件名
//-----------------------------------------------------------------------------
void
CXModel::RemovePathFromFileName(LPSTR fullPath, LPWSTR fileName)
{
	//先将fullPath的类型变换为LPWSTR
	WCHAR wszBuf[MAX_PATH];
	MultiByteToWideChar( CP_ACP, 0, fullPath, -1, wszBuf, MAX_PATH );
	wszBuf[MAX_PATH-1] = L'\0';

	WCHAR* wszFullPath = wszBuf;

	//从绝对路径中提取文件名 , zhh注：若归于子目录中时，为便于管理需注释下面的三行
	LPWSTR pch=wcsrchr(wszFullPath,'\\');
	//if (pch)
	//	lstrcpy(fileName, ++pch);
	//else
		lstrcpy(fileName, wszFullPath);
}

VOID
CXModel::Cleanup()
{
	//释放网格模型材质
    if( g_pMeshMaterials != NULL ) 
        delete[] g_pMeshMaterials;

	//释放网格模型纹理
    if( g_pMeshTextures )
    {
        for( DWORD i = 0; i < g_dwNumMaterials; i++ )
        {
            if( g_pMeshTextures[i] )
                g_pMeshTextures[i]->Release();
        }
        delete[] g_pMeshTextures;
    }

	//释放网格模型对象
    if( g_pMesh != NULL )
        g_pMesh->Release();

}

//-----------------------------------------------------------------------------
// Desc: 渲染场景
//-----------------------------------------------------------------------------
VOID
CXModel::Render()
{

	//if (this->isCustom)
	//{
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, TRUE );
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0x80ffffff);
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );
	//	//if (drawMode==纯色模式)
	//	//{
	//	//	IFC(m_pd3dDevice->SetTexture(0,nullptr));
	//	//	m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,mtrl.Diffuse.a==1); //zh深度设置, 若为半透，需设为false
	//	//	myTri->m_pd3dDevice->SetMaterial( &mtrl );
	//	//}
	//	//else if (drawMode==纹理模式)
	//	//{
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,true);
	//	CGlobal::pCurRender->m_pd3dDevice->SetTexture(0,g_pTexture);
	//	//}
	//	//else if (drawMode==线框模式)
	//	//{
	//	//	IFC(m_pd3dDevice->SetTexture(0,nullptr));
	//	//	myTri->m_pd3dDevice->SetMaterial( &mtrl );
	//	//	m_pd3dDevice->SetRenderState(D3DRS_FILLMODE,D3DFILL_WIREFRAME);

	//	//}

	//	mesh->DrawSubset(0);

		//CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CW );
	//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_FILLMODE,D3DFILL_SOLID);

	//}
	//else  // x文件模型的显示
	{


		CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ZWRITEENABLE,true);

		CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHATESTENABLE, true);
		//CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHAREF, 0x00000000);
		//CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHAFUNC, D3DCMP_GREATER);


		CGlobal::pCurRender->m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_TEXTURE);

		CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_NONE );





		//	CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHAFUNC, D3DCMP_NEVER);
		//CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHATESTENABLE, false);
		//CGlobal::pCurRender->m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_DIFFUSE);
		//CGlobal::pCurRender->m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,false);
		//CGlobal::pCurRender->m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG1);


	//		 //设置材质
 //   D3DMATERIAL9 mtrl;
 //   ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
 //   mtrl.Diffuse.r = mtrl.Ambient.r = 0;
 //   mtrl.Diffuse.g = mtrl.Ambient.g = 0;
 //   mtrl.Diffuse.b = mtrl.Ambient.b = 1;
 //   mtrl.Diffuse.a = mtrl.Ambient.a = 0.8;

	//mtrl.Specular.r=mtrl.Specular.g=mtrl.Specular.b=0.00025;
	//mtrl.Specular.a=1;
	//mtrl.Power=5;
	//CGlobal::pCurRender->m_pd3dDevice->SetMaterial( &mtrl );

	// //设置灯光
 //   D3DXVECTOR3 vecDir;
 //   D3DLIGHT9 light;
 //   ZeroMemory( &light, sizeof(D3DLIGHT9) );
 //   light.Type       = D3DLIGHT_DIRECTIONAL;
 //   light.Diffuse.r  =1;
 //   light.Diffuse.g  = 1;
 //   light.Diffuse.b  = 1;
	//light.Ambient.r=0;
	//light.Ambient.g=0;
	//light.Ambient.b=0;
	//light.Specular.r=light.Specular.g=light.Specular.b=0.0001;
	//vecDir = D3DXVECTOR3(1.0f, 1.0f, 0.0f);
	//////vecDir = D3DXVECTOR3(-0.96580493 ,0.011636866 ,0.25900859);
 //   D3DXVec3Normalize( (D3DXVECTOR3*)&light.Direction, &vecDir );
 //   //light.Range       = 1000.0f;
 //   CGlobal::pCurRender->m_pd3dDevice->SetLight( 0, &light );
 //   CGlobal::pCurRender->m_pd3dDevice->LightEnable( 0, true );
	//CGlobal::pCurRender->m_pd3dDevice->LightEnable( 1, false );
	//CGlobal::pCurRender->m_pd3dDevice->LightEnable( 2, false );
	//CGlobal::pCurRender->m_pd3dDevice->LightEnable( 3, false );
	//CGlobal::pCurRender->m_pd3dDevice->LightEnable( 4, false );
	//CGlobal::pCurRender->m_pd3dDevice->LightEnable( 5, false );
    //CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, true );
	//CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_SPECULARENABLE, true );
 //   //设置环境光
 //   CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_AMBIENT, 0xFF101010);






		//逐块渲染网格模型
		for( DWORD i=0; i<g_dwNumMaterials; i++ )
		{
			//设置材质和纹理
			if (g_pMeshMaterials!=NULL)
				CGlobal::pCurRender->m_pd3dDevice->SetMaterial( &g_pMeshMaterials[i] );
			CGlobal::pCurRender->m_pd3dDevice->SetTexture( 0, g_pMeshTextures[i] );
			//渲染模型
			g_pMesh->DrawSubset( i );


		}
			CGlobal::pCurRender->m_pd3dDevice->SetTexture(0,nullptr);


		CGlobal::pCurRender->m_pd3dDevice->SetRenderState( D3DRS_CULLMODE, D3DCULL_CW );

	}






	





}