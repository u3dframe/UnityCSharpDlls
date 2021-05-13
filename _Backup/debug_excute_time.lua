--- 工具 - 时间打印
-- Author : canyon / 龚阳辉
-- Date : 2021-04-28 17：25
-- Desc : require("luaex/debug_excute_time")
local tb_insert = table.insert
local tb_concat = table.concat
local str_fmt = string.format
local os_clock = os.clock
local M = {}
M.__index = M
function M.New(o)
	o = (type(o) == "table") and o or {}
	o.tags = o.tags or {}
	setmetatable(o,M)
	return o
end

local __single = nil
function M.singler()
	if not __single then
		__single = M.New()
	end
	return __single
end

function M:Record( tag,cmd,isEnd )
	if not tag then
		return
	end
	
	local _t = self.tags[tag] or {}
	self.tags[tag] = _t
	if cmd then
		tb_insert( _t,{ cmd = cmd,otime = os_clock() } )
	end
	
	if isEnd == true then
		self.tags[tag] = nil
		local _tt,_t1 = {"=== ",tag,"\r\n"}
		local _lasttime,_firsttime,_diff = -1,-1
		for i = 1,#_t do
			_t1 = _t[i]
			tb_insert( _tt,"cmd = " )
			tb_insert( _tt,_t1.cmd )
			tb_insert( _tt," , time = " )
			tb_insert( _tt,_t1.otime )
			tb_insert( _tt," , diff_sec = " )
			if _lasttime == -1 then
				_firsttime = _t1.otime
				_lasttime = _firsttime
			end
			_diff = (_t1.otime - _lasttime)
			tb_insert( _tt,_diff <= 0 and 0 or _diff)
			tb_insert( _tt,"\r\n" )
			_lasttime = _t1.otime
		end
		tb_insert( _tt,"all_diff_sec = " )
		_diff = (_lasttime - _firsttime)
		tb_insert( _tt,_diff)
		tb_insert( _tt,"\r\n" )
		local _str = tb_concat( _tt,"")
		local _pt = printInfo or print
		_pt( _str )
	end
end

function RecordTime( tag,cmd,isEnd )
	local _obj = M.singler()
	_obj:Record( tag,cmd,isEnd )
end

return M