#include "StdAfx.h"
#include "Parrow.h"

CParrow::CParrow(void)
{
}


CParrow::~CParrow(void)
{
}


HRESULT
CParrow::Create(IDirect3DDevice9 *m_pd3dDevice, CBasicModel **ppModel, int id , CPline *parent,CTriangleRenderer* tri, int idx, int count)
{
	HRESULT hr = S_OK;

	CParrow *pModel = new CParrow();
    IFCOOM(pModel);
	pModel->myTri=tri;
	pModel->idx=idx;
	pModel->count=count;

	pModel->id=id;
    IFC(pModel->Init(m_pd3dDevice,id,parent));  //直接创建

    *ppModel = pModel;
    pModel = NULL;

Cleanup:
    delete pModel;

    return hr;

}

HRESULT
CParrow::Init(IDirect3DDevice9 *m_pd3dDevice, int id , CPline *parent)
{
	HRESULT hr = S_OK;
	this->parent=parent;
	isInner=true;

	//定义顶点
	CUSTOMVERTEX_C* vertices(nullptr);
	vertices=new CUSTOMVERTEX_C[3];
	vertices[0].position=D3DXVECTOR3(0,1,0);vertices[0].color=parent->arrowColor;
	vertices[2].position=D3DXVECTOR3(1,-1,0);//vertices[0].color=0xff0000ff;
	vertices[1].position=D3DXVECTOR3(-1,-1,0);//vertices[0].color=0xff0000ff;

	NumVertices=3; //顶点数
	PrimCount=1; //三角数

	//===== 顶点缓冲	
	IFC(m_pd3dDevice->CreateVertexBuffer(NumVertices*sizeof(CUSTOMVERTEX_C), 0, D3DFVF_CUSTOMVERTEX_C, D3DPOOL_DEFAULT, &m_pd3dVB, NULL)); 
	void *pVertices;
	IFC(m_pd3dVB->Lock(0, NumVertices*sizeof(CUSTOMVERTEX_C), &pVertices, 0));
	memcpy(pVertices, vertices, NumVertices*sizeof(CUSTOMVERTEX_C));
	m_pd3dVB->Unlock();


	//============================ 其它计算和参数设定 ==========================================
	float size=parent->arrowSize;
	location=parent->points[0];
	initBaseWorld();//计算baseworld
	
	memcpy(&world,&baseWorld,sizeof(baseWorld));
	
	D3DXVECTOR3 tmp1,tmp2;
	tmp1=D3DXVECTOR3(0,1,0);
	tmp2=D3DXVECTOR3(0,0,0);
	D3DXVec3TransformCoord(&tmp1,&tmp1,&baseWorld);
	D3DXVec3TransformCoord(&tmp2,&tmp2,&baseWorld);
	if(parent->isInverse)
		lastDir=tmp2-tmp1;
	else
		lastDir=tmp1-tmp2;
	D3DXVec3Normalize(&lastDir,&lastDir);
	memcpy(&lastPoint,&parent->points[0],sizeof(lastPoint));



Cleanup:
	delete vertices;
    return hr;
}

void
CParrow::initBaseWorld()
{
	float size=parent->arrowSize;
	D3DXMatrixIdentity(&baseWorld);
	D3DXMatrixScaling(&baseWorld,size,size,size);

	if (myTri->earthpara.SceneMode==地球)
	{
		D3DXVECTOR3 location2 =D3DXVECTOR3(0,0, myTri->para.Radius+myTri->para.LineHeight);// new Vector3(0, 0, Para.Radius + Para.LineHeight);
		D3DXMATRIXA16 tmpmatrix;
		D3DXMatrixTranslation(&tmpmatrix,location2.x,location2.y,location2.z);
		D3DXMatrixMultiply(&baseWorld,&baseWorld,&tmpmatrix);

		D3DXVECTOR3 tmp=D3DXVECTOR3(location.x,location.y,location.z);
		D3DXVec3Normalize(&tmp,&tmp);
		D3DXVECTOR3 location3 = location + tmp * (myTri->para.ArrowHeight - myTri->para.LineHeight);

		tmpmatrix=CHelper::getMatrixP2P(location2,location3);
		D3DXMatrixMultiply(&baseWorld,&baseWorld,&tmpmatrix);
	}
	else
	{
		D3DXMATRIXA16 tmpmatrix;
		D3DXMatrixTranslation(&tmpmatrix, location.x,location.y, location.z);
		baseWorld*=tmpmatrix;
	}

	if(parent->isInverse)
	{
		D3DXMATRIXA16 matrix;
		D3DXMatrixRotationZ(&matrix,D3DX_PI);
		baseWorld = matrix * baseWorld;
	}
}

void
CParrow::updateWorld()
{
	UINT  iTime  = GetTickCount() % parent->aniFlow.duration;
	float process = (float)iTime / parent->aniFlow.duration;

	//多箭头重算process
	process=1.0f*idx/count+process/count;
	if (parent->isInverse)
		process=1.0f-process;


	//计算新位置
	D3DXVECTOR3 newpoint;
	float len = 0;
	for (int i = 0; i < parent->pointCount - 1; i++)
	{
		float tmp = len +parent->lens[i];
		if (tmp /parent->lenth > process)
		{
			float tmp2 = process * parent->lenth - len;
			newpoint = parent->points[i] + parent->dirs[i] * tmp2;

			D3DXVECTOR3 tmpdir = newpoint;
			D3DXVec3Normalize(&tmpdir,&tmpdir);
			newpoint = newpoint + tmpdir * (myTri->para.ArrowHeight - myTri->para.LineHeight);

			D3DXMATRIXA16 tranlate;
			D3DXMatrixTranslation(&tranlate,newpoint.x-lastPoint.x,newpoint.y-lastPoint.y,newpoint.z-lastPoint.z);
			world *= tranlate;
			lastPoint=newpoint;

			if (parent->dirs[i]!=lastDir)
			{
				D3DXVECTOR3 vaxis;
				D3DXVec3Cross(&vaxis,&parent->dirs[i], &lastDir);
				D3DXVec3Normalize(&vaxis,&vaxis);

				float angle= CHelper::getAngle(parent->dirs[i],lastDir);


				float temp =D3DXVec3Dot(&vaxis, &newpoint) / D3DXVec3Length(&vaxis) / D3DXVec3Length(&newpoint);
				temp = temp > 1 ? 1 : temp;
				temp = temp < -1 ? -1 : temp;
				float checkangle = acos(temp);
				float angleadd = checkangle >D3DX_PI/2 ? 1 : -1;
				D3DXMATRIXA16 matrix;
				D3DXMatrixRotationZ(&matrix,angleadd * angle);
				world = matrix * world;

				lastDir =parent-> dirs[i];

			}

			break;
		}
		len = tmp;
	}


}


HRESULT
CParrow::Render(IDirect3DDevice9 *m_pd3dDevice)
{
	HRESULT hr = S_OK;

	//if (idx!=2)
	//	return hr;

	if (parent->aniFlow.isDoAni)
	{
		updateWorld();
		IFC(m_pd3dDevice->SetTransform(D3DTS_WORLD, &world));

		m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, false );

		IFC(m_pd3dDevice->SetTexture(0,NULL));
		IFC(m_pd3dDevice->SetStreamSource(0, m_pd3dVB, 0, sizeof(CUSTOMVERTEX_C)));
		IFC(m_pd3dDevice->SetFVF(D3DFVF_CUSTOMVERTEX_C));

		IFC(m_pd3dDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, PrimCount));   //色彩示例用

		m_pd3dDevice->SetRenderState( D3DRS_LIGHTING, true );
	}

Cleanup:
    return hr;

}

