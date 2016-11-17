#pragma once

class CTriangleRenderer;

static class CGlobal
{
public:
	CGlobal(void);
	~CGlobal(void);

	static CTriangleRenderer* pCurRender;

};

