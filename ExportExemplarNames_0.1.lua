-- ===================================================================================================================
<<<<<<< Updated upstream
-- Name: main
-- Desc: Writes Type, Group, Instance, Exemplar name as comma separated list to text file
-- Params: 	N/A
-- Called by: N/A
-- Comments: N/A
--====================================================================================================================
=======
-- Name: 		reader.main
-- Desc: 		writes Type, Group, Instance, Exemplar name as comma separated list to text file
-- Params: 		N/A
-- Called by: 	N/A
-- Comments: 	N/A
-- ===================================================================================================================
>>>>>>> Stashed changes
function reader.main (this)   
    local file = io.open("C:/Users/Administrator/OneDrive/SC4 Deps/SC4PropTextureCatalog/working/exemplarexport.txt", "w")
    entryidx = reader:entries_GetHeadPosition()     
    while entryidx~=0 do
        entry,entryidx = reader:entries_GetNext(entryidx)
        flag = reader:entry_GetFlag(entry)  
        if (flag == 7) then
            exemplar = reader:ex_Decode(entry) 
            exemplaridx = reader:exprop_GetHeadPosition(exemplar)  
            while exemplaridx~=0 do
                exprop, exemplaridx = reader:exprop_GetNext(exemplar,exemplaridx)
                desc, descname = reader:exprop_GetDesc(exprop)  
                valuestr = reader:exprop_GetValueStr(exprop)
				T,G,I = reader:entry_GetTGI(entry)
                if (descname == "Exemplar Name") then
                    file:write(Dec2Hex(T,8),",",Dec2Hex(G,8),",",Dec2Hex(I,8),",",valuestr,"\n")
					break
                end
            end
        end
    end
    reader:refresh()
    file:close()
end


-- ===================================================================================================================
<<<<<<< Updated upstream
-- Name: Dec2Hex
-- Desc: Returns a hex number prepended (if necessary) with zeros up to a specified length
-- Params: 	dec ... decimal number to convert
-- 			hexLen ... length to prepend zeros if hex is shorter than specified length (usually 8)
-- Called by: reader.main
-- Comments: N/A
--====================================================================================================================
=======
-- Name:		Dec2Hex
-- Desc: 		Returns a hex number prepended (if necessary) with zeros up to a specified length
-- Params: 		dec ... decimal number to convert
-- 				hexLen ... length to prepend zeros if hex is shorter than specified length (usually 8)
-- Called by: 	reader.main
-- Comments: 	N/A
-- ===================================================================================================================
>>>>>>> Stashed changes
function Dec2Hex(dec,hexLen)
    hex = string.rep("0", (hexLen - (string.len(string.format("%x", dec))))) .. string.format("%x", dec)
    return hex
end