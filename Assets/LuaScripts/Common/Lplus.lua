local _class={}--全局类容器 -- 所有的类 从游戏开始之后所有的类表

local Class = function(base)
	local TemplateTable={}--在此类上操作，最后返回
	TemplateTable.ctor = false--构造 方法
	TemplateTable.base = base--基类
	_class[TemplateTable]={}--放入类容器
	local vtbl = _class[TemplateTable];--创建一个基础元表
	
	TemplateTable.new = function(...)--new 方法
		local obj={}   ----new 方法里创建出一个类型的实例或者叫对象
		setmetatable(obj,{ __index = vtbl })--?
		
		do
			local create --确保调用一个class的new的时候调用到“基类”的new。一个递归，找到所有基类，调用一下所有类的new
			create = function(this,...)
				if this.base then--有基类就重新检查一遍基类有没有构造
					create(this.base,...)
				end
				if this.ctor then--无论当前的C是派生类还是基类，有用户赋值的构造就调用
					this.ctor(obj,...)
				end
			end
		
			create(TemplateTable,...)--进入递归
		end 
		
		return obj
	end

	setmetatable(TemplateTable,{__newindex=	--注意只赋值了newindex
		function(t,k,v)
			vtbl[k]=v
		end
			
	})
	
	if base~=nil then--如果有基类
		--先设置一个__index，一个方法，这个方法是找基类！！
		setmetatable(vtbl,{__index=
			function(t,k)
				local ret=_class[base][k]
				vtbl[k]=ret
				return ret
			end
		})
	end
 	return TemplateTable
end

return  Class

 