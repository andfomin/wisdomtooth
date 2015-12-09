// dllmain.h : Declaration of module class.

class CIEHelperModule : public ATL::CAtlDllModuleT< CIEHelperModule >
{
public :
	DECLARE_LIBID(LIBID_IEHelperLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_IEHELPER, "{1BBF80BF-3FE1-4813-B05B-359C29F1972E}")
};

extern class CIEHelperModule _AtlModule;
