local debug,tostring,type,getmetatable = debug,tostring,type,getmetatable
local str_format = string.format
local tb_insert = table.insert
local unpack = unpack or table.unpack
local m_maxinteger = math.maxinteger

local INFO = {
    ["function"] = function(v)
        local info = debug.getinfo(v)
        local src = info.short_src
        local line = info.linedefined
        return str_format("%s--[[%s:%d]]", v, src, line)
    end,
    ["table"] = function(v)
        local mt = getmetatable(v)
        if not mt then return tostring(v) end
        local __tostring = mt.__tostring
        mt.__tostring = nil
        local r = tostring(v)
        mt.__tostring = __tostring
        return r
    end,
    ["userdata"] = function(v)
        local mt = getmetatable(v)
        if not mt then return tostring(v) end
        local __tostring = mt.__tostring
        mt.__tostring = nil
        local r = tostring(v)
        mt.__tostring = __tostring
        return r
    end
}

local function infostring(v)
    local types = type(v)
    local call = INFO[types] or tostring
    return call(v), types
end

local PROXY = {
    ["table"] = function(v)
        local ret = {}
        for k, _v in next, v do tb_insert(ret, {k, _v}) end
        local mt = getmetatable(v)
        if mt then tb_insert(ret, {"__metatable", mt}) end
        return ret
    end,
    ["function"] = function(v)
        local ret = {}
        for i = 1, m_maxinteger do
            local name, value = debug.getupvalue(v, i)
            if not name then break end
            tb_insert(ret, {name, value})
        end
        return ret
    end,
    ["userdata"] = function(v)
        local mt = getmetatable(v)
        if mt then
            return {{"__metatable", mt}}
        else
            return {}
        end
    end,
    ["thread"] = function(v)
        local ret = {}
        for i = 0, m_maxinteger do
            local info = debug.getinfo(v, i, "flnStu")
            if not info then break end
            local key = str_format("%d %s %s(%s:%d)", i, info.what or "nil",
                                      info.name, info.short_src,
                                      info.currentline)
            tb_insert(ret, {key, info.func})
            for j = 1, m_maxinteger do
                local name, val = debug.getlocal(v, i, j)
                if not name then break end
                tb_insert(ret, {str_format("-->%d local %s", j, name), val})
            end
        end
        return ret
    end
}

local function tblinfo(tbl)
    local ret = {}
    for _, v in pairs(tbl) do
        tb_insert(ret, {infostring(v[1]), infostring(v[2])})
    end
    return ret
end

local function vminfo(root, ...)
    local keys = {}
    local function getsub(S, K, ...)
        if K == nil then return tblinfo(S) end
        tb_insert(keys, K)
        local s
        for _, v in ipairs(S) do
            if infostring(v[1]) == K then
                s = v[2]
                break
            end
            -- if infostring(v) == K then
            --     s = v
            --     break
            -- end
        end

        local proxy = PROXY[type(s)]
        if proxy then
            return getsub(proxy(s), ...)
        else
            return infostring(s)
        end
    end
    return keys, getsub(PROXY.table(root or {}), ...)
end

local json = require "cjson.safe"
if MgrNet then
    MgrNet.AddPushCall( "debug_vmregister", function(msg)
        local session = msg.session
        local keys, vals = vminfo(debug.getregistry(), unpack(msg.keys))
        local result = json.encode({keys = keys, vals = vals})
        while #result > 32657 do
            MgrNet.SendRequest("debug_longstring", {session = session, data = result:sub(1, 32657)})
            result = result:sub(32658)
        end
        MgrNet.SendRequest("debug_longstring_over", {session = session, data = result})
    end)
else
    -- 加入封装的 socket
    
end