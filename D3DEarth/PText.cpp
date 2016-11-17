#include "StdAfx.h"
#include "PText.h"


CPText::CPText(void)
{
}


CPText::~CPText(void)
{
}

HRESULT
CPText::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , void* ppara, void* pmesh, int mcount, void* ptext, int tcount, CTriangleRenderer* tri)
{
	HRESULT hr = S_OK;

	CPText *pModel = new CPText();
    IFCOOM(pModel);
	pModel->myTri=tri;

	pModel->id=id;
    IFC(pModel->Init(m_pd3dDevice, ppara, pmesh, mcount, ptext, tcount));  //直接创建

    *ppModel = pModel;
    pModel = NULL;

Cleanup:
    delete pModel;

    return hr;
}


typedef vector<wstring> str_container_type;
void split(wstring const& str, wstring const& delimiter, str_container_type& dest)
{
   size_t pos = 0, found = 0;
   while ( found != string::npos )
   {
       found = str.find(delimiter, pos);
       dest.push_back(wstring(str, pos, found - pos));
       pos = found + 1;
    }
}




HRESULT
CPText::Init(IDirect3DDevice9 *m_pd3dDevice, void* ppara, void* pmesh, int mcount, void* ptext, int tcount)
{
	HRESULT hr = S_OK;
	this->is3D=true;


	STRUCT_Symbol* para=(STRUCT_Symbol*)ppara;
	this->isReceivePick=para->isReceivePick;
	this->pickFlagId=para->pickFlagId;
	this->deepOrd=para->deepOrd;
	this->rootid=para->rootid;
	this->parentid=para->parentid;

	this->isH=para->isH;
	this->scalex=para->scaleX;
	this->scaley=para->scaleY;
	this->texturekey=para->texturekey;
	//this->color=para->color;
	memcpy(&mtrl,&para->material,sizeof(mtrl));
	this->axis=para->axis;
	this->angle=para->angle;
	
	D3DXVECTOR3* pd=(D3DXVECTOR3*)pmesh;
	memcpy(&location,pd,sizeof(location)); //复制

	wstring s((WCHAR*)ptext);
	split(s,L"|",text);

	if (id != rootid) //非根, 置父对象指针
		pparent=(CPPolyCol*)CPPolyCol::find(rootid,parentid,myTri);
	else
		pparent=nullptr;


	CUSTOMVERTEX_T* vertices(nullptr);
	vertices=new CUSTOMVERTEX_T[4];
	if (isH)//水平
	{
		vertices[0].position= D3DXVECTOR3(1,1,0); vertices[0].normal=D3DXVECTOR3(0,0,1); vertices[0].u=1; vertices[0].v=0;
		vertices[1].position= D3DXVECTOR3(-1,1,0); vertices[1].normal=D3DXVECTOR3(0,0,1); vertices[1].u=0; vertices[1].v=0;
		vertices[2].position= D3DXVECTOR3(1,-1,0); vertices[2].normal=D3DXVECTOR3(0,0,1); vertices[2].u=1; vertices[2].v=1;
		vertices[3].position= D3DXVECTOR3(-1,-1,0); vertices[3].normal=D3DXVECTOR3(0,0,1); vertices[3].u=0; vertices[3].v=1;
	}
	else //垂直地面
	{
		vertices[0].position= D3DXVECTOR3(1,0,2); vertices[0].normal=D3DXVECTOR3(0,1,0); vertices[0].u=1; vertices[0].v=0;
		vertices[1].position= D3DXVECTOR3(-1,0,2); vertices[1].normal=D3DXVECTOR3(0,1,0); vertices[1].u=0; vertices[1].v=0;
		vertices[2].position= D3DXVECTOR3(1,0,0); vertices[2].normal=D3DXVECTOR3(0,1,0); vertices[2].u=1; vertices[2].v=1;
		vertices[3].position= D3DXVECTOR3(-1,0,0); vertices[3].normal=D3DXVECTOR3(0,1,0); vertices[3].u=0; vertices[3].v=1;
	}
	NumVertices=4; //顶点数
	PrimCount=2; //三角数

	//===== 顶点缓冲	
	IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_T), 0, D3DFVF_CUSTOMVERTEX_T, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	void *pVertices;
	IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_T), &pVertices, 0));
	memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_T));
	m_pd3dVB->Unlock();
	//===== 纹理
	if (is3D) //  3d 模式
	{
		CreateTexture(m_pd3dDevice);  //生成文字纹理

		if (texturekey!=0)  //获取背景纹理
		{
			auto iter=myTri->para.textures.find(texturekey);
			if (iter!=myTri->para.textures.end())
			{
				g_pTexture2=iter->second;
				isDicTexture2=true;
			}

		}
	}
	//IFC(D3DXCreateTextureFromFile(m_pd3dDevice,L"2.jpg",&g_pTexture));






	calWorld();


Cleanup:
	delete vertices;
    return hr;

}

HRESULT
CPText::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	/*D3DMATERIAL9 mtrl;
    ZeroMemory( &mtrl, sizeof(D3DMATERIAL9) );
    mtrl.Diffuse.r = mtrl.Ambient.r=mtrl.Emissive.r = 0.990f;
    mtrl.Diffuse.g = mtrl.Ambient.g=mtrl.Emissive.g = 0.990f;
    mtrl.Diffuse.b = mtrl.Ambient.b=mtrl.Emissive.b = 0.990f;
    mtrl.Diffuse.a = mtrl.Ambient.a = 1.0f;
    CPara::pCurRender->m_pd3dDevice->SetMaterial( &mtrl );*/
	m_pd3dDevice->SetMaterial( &myTri->basicmtrl );

	m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_TEXTURE);
	m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG1);


	HRESULT hr = S_OK;

	if (is3D) //  3d 模式
	{
	//设置纹理采样方式
	m_pd3dDevice->SetSamplerState(0, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
	m_pd3dDevice->SetSamplerState(0, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
	//m_pd3dDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);


	//设置纹理混合
	if (texturekey!=0) //双纹理
	{
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_TEXCOORDINDEX,0);
		m_pd3dDevice->SetTextureStageState(1,D3DTSS_TEXCOORDINDEX,0);

		m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLORARG1,D3DTA_TEXTURE);
		m_pd3dDevice->SetTextureStageState(0,D3DTSS_COLOROP,D3DTOP_SELECTARG1);


		m_pd3dDevice->SetTextureStageState(1,D3DTSS_COLORARG1,D3DTA_TEXTURE);
		m_pd3dDevice->SetTextureStageState(1,D3DTSS_COLORARG2,D3DTA_CURRENT);
		m_pd3dDevice->SetTextureStageState(1,D3DTSS_COLOROP,D3DTOP_BLENDTEXTUREALPHA);

		//设置纹理1采样方式
		m_pd3dDevice->SetSamplerState(1, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
		m_pd3dDevice->SetSamplerState(1, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);

		//设置半透混合
		m_pd3dDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);
		m_pd3dDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);
		m_pd3dDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);

		IFC(m_pd3dDevice->SetTexture(0,g_pTexture2));
		IFC(m_pd3dDevice->SetTexture(1,g_pTexture));
	}
	else //仅文字纹理
	{
		IFC(m_pd3dDevice->SetTexture(0,g_pTexture));
	}






		IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));

		
		IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_T)));
		IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_T));
		IFC(m_pd3dDevice->DrawPrimitive(D3DPT_TRIANGLESTRIP, 0, PrimCount));   

		//清理
		if (texturekey!=0) //双纹理
		{
			IFC(m_pd3dDevice->SetTexture(0,nullptr));
			IFC(m_pd3dDevice->SetTexture(1,nullptr));
		}
		else  //仅文字纹理
		{
			IFC(m_pd3dDevice->SetTexture(0,nullptr));
		}



		D3DXMATRIXA16 tmp;
		D3DXMatrixIdentity(&tmp);
		IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &tmp));

		


		//mtrl.Emissive.a=mtrl.Emissive.r=mtrl.Emissive.g=mtrl.Emissive.b=0.0f;
		//CPara::pCurRender->m_pd3dDevice->SetMaterial( &mtrl );
	}
	else //  2D 模式
	{
		//计算世界空间位置
		D3DXVECTOR3 loca;
		float distanceGround;
		if (pparent!=nullptr) //有父对象
			distanceGround = pparent->distanceGround + pparent->height;
		else
			distanceGround=myTri->para.TextHeight;
		float len=D3DXVec3Length(&location);
		D3DXVec3Scale(&loca,&location,(1.0f+distanceGround/len));
		//计算距相机距离
		float distance=D3DXVec3Length(&(loca-myTri->camera->curpara.cameraPosition));
		if (distance<5)
		{
			RECT rect=getScreenRect(loca); //计算渲染文字矩形
			if (rect.left<myTri->para.D3DWidth && rect.right>0 && rect.top<myTri->para.D3DHeight && rect.bottom>0)
			{
					for(auto iter=text.begin();iter!=text.end();iter++)
					{
						D3DXCOLOR color=D3DXCOLOR(mtrl.Ambient);
					    myTri->para.g_pFont2D->DrawText(NULL, iter->c_str(), iter->length(), &rect,DT_SINGLELINE|DT_NOCLIP|DT_CENTER|DT_VCENTER, color);
						rect.top+=22;
						rect.bottom+=22;
					}
				
			}
		}

	}

Cleanup:

    return hr;

}

void
CPText::SetEffect(IDirect3DDevice9 *m_pd3dDevice,CTriangleRenderer* myTri)
{
	

}


void
CPText::calWorld()
{
	//位移到指定经纬度高
	float distanceGround;
	if (pparent!=nullptr) //有父对象
		distanceGround = pparent->distanceGround + pparent->height;
	else
		distanceGround=myTri->para.TextHeight;

	D3DXMATRIXA16 tmp;
	D3DXMatrixScaling(&world,width,height,height);
	if (myTri->earthpara.SceneMode==地球)
	{
		D3DXVECTOR3 location2=D3DXVECTOR3(0,0,myTri->para.Radius+distanceGround);
		D3DXMatrixTranslation(&tmp,0,0,myTri->para.Radius+distanceGround);
		world*=tmp;
		tmp=CHelper::getMatrixP2P(location2,location);
	}
	else
	{
		D3DXMatrixTranslation(&tmp, location.x,location.y, distanceGround);
	}
	world*=tmp;

}

//---------------------------------
// 创建纹理
//---------------------------------
HRESULT
CPText::CreateTexture(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;
	
	//判断字符数
	setlocale(LC_ALL, "Chinese-simplified");
	int  charcount=0;
	for(auto iter=text.begin();iter!=text.end();iter++)
	{
		int tmpi=wcstombs( NULL,iter->c_str(), 0);
		charcount= charcount>tmpi?charcount:tmpi;
	}

	float charsize=0.0075f;
	width=charcount*charsize*scalex;
	height=text.size()*(2.5+2/50)*charsize*scaley;
	//height=charsize*scalex;
	//width=height/52*text.size()*512*scaley;

	//UINT fontwidth=(UINT)(512/(charcount+1));
	//UINT fontheight=512;

	float ww=28*charcount;
	float hh=52*text.size();

	// Create texture
	IFC(m_pd3dDevice->CreateTexture(
		ww,
		hh,
		0,
		D3DUSAGE_RENDERTARGET,
		D3DFMT_A8R8G8B8,
		D3DPOOL_DEFAULT,
		&g_pTexture,
		NULL));
	IDirect3DSurface9*        g_pRenderSurface    = NULL ;

	// Get texture surface
	IFC(g_pTexture->GetSurfaceLevel(0, &g_pRenderSurface)) ;
	IDirect3DSurface9* g_pOldRenderTarget;
	m_pd3dDevice->GetRenderTarget(0, &g_pOldRenderTarget) ;

	//LPD3DXFONT g_pFont = 0;       //字体对象
	//D3DXCreateFont(m_pd3dDevice,fontheight,fontwidth,0,5,0,0,0,0,0, L"Arial", &g_pFont);


	





	RECT clientRect;
	clientRect.left=clientRect.top=0;
	clientRect.right=ww;

	clientRect.bottom=50;//512/text.size();



	// Set texture surface as RenderTarget
	m_pd3dDevice->SetRenderTarget(0, g_pRenderSurface) ;
	m_pd3dDevice->Clear( 0, NULL, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, 0x00000000, 1.0f, 0 );

	m_pd3dDevice->BeginScene();
	{


		for(auto iter=text.begin();iter!=text.end();iter++)
		{
			D3DXCOLOR color=D3DXCOLOR(mtrl.Ambient);
			myTri->para.g_pFont3D->DrawText(NULL, iter->c_str(), iter->length(), &clientRect,DT_SINGLELINE|DT_NOCLIP|DT_CENTER|DT_VCENTER, color);
			clientRect.top+=52;//512/text.size();
			clientRect.bottom+=52;//512/text.size();
		}

	}

	m_pd3dDevice->EndScene();

	m_pd3dDevice->SetRenderTarget(0, g_pOldRenderTarget) ;

Cleanup:
	//g_pFont->Release();
    return hr;
}

//---------------------------------
// 动态计算屏幕空间中的文字渲染矩形
//---------------------------------
RECT
CPText::getScreenRect(D3DXVECTOR3 loca)
{

	POINT screenpoint=CHelper::GetProjectPoint2D(loca,myTri->camera->view,myTri->camera->projection,myTri->para.D3DWidth,myTri->para.D3DHeight);

	setlocale(LC_ALL, "Chinese-simplified");
	int  charcount=0;
	for(auto iter=text.begin();iter!=text.end();iter++)
	{
		int tmpi=wcstombs( NULL,iter->c_str(), 0);
		charcount= charcount>tmpi?charcount:tmpi;
	}
	int rectwidth=charcount*8;
	int rectheight=20;// (20+2)*text.size();
	
	RECT rect;
	rect.left=screenpoint.x-rectwidth/2;
	rect.right=screenpoint.x+rectwidth/2;
	rect.top=screenpoint.y-rectheight;
	rect.bottom=screenpoint.y;

	return rect;
}


void
CPText::ChangeProperty(EPropertyType propertytype,void* para,int count)
{
	if (propertytype==内容)
	{
		text.clear();
		wstring s((WCHAR*)para);
		split(s,L"|",text);
		if (is3D) //  3d 模式
			CreateTexture(myTri->m_pd3dDevice);  //生成文字材质
	}
	else if (propertytype==材质)
	{
		//memcpy(&color,(DWORD*)para,sizeof(color));
		memcpy(&mtrl,(D3DMATERIAL9*)para,sizeof(mtrl));
		if (is3D) //  3d 模式
			CreateTexture(myTri->m_pd3dDevice);  //生成文字材质
	}
}



