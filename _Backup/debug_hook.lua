--- 工具 - 环境检查
-- Author : canyon / 龚阳辉
-- Date : 2021-04-28 17：25
-- Desc : require("luaex/debug_hook")

local error,debug = error,debug
local str_fmt = string.format
local _declaredNames =  { CDebug = true }
setmetatable(_G,{
	__newindex = function(t,k,v)
		if not _declaredNames[k] then
			local _w = debug.getinfo(2,"S").what
			if _w ~= "main" and _w ~= "C" then
				local _s = str_fmt("attempt to write to undeclared variale = [%s] ",k)
				error(_s,2)
			end
			_declaredNames[k] = true
		end
		rawset(t,k,v)
	end,
	__index = function(_,k)
		if not _declaredNames[k] then
			local _s = str_fmt("attempt to read to undeclared variale = [%s] ",k)
			error(_s,2)
		else
			return nil
		end
	end,
})


