------------------------------------------------------------------------------------------------
--                   Cori's Lua Code for Reader 1.5.4 - code version 1.00                     --
--                                                                                            --
--  File name: Cori's Cohort and Exemplar Properties Script v1.00.lua  as of 26 August, 2020  --
------------------------------------------------------------------------------------------------

-- WHAT IS THIS? --
-------------------
--  This Lua code is to be used in Script Mode of Reader 1.5.4. You can load any data file that Reader understands, execute this script
--    and it will output all of the Cohorts and Exemplar Properties to a text file. This code does not make any changes to the data file.

-- WHAT IS THE MOST *** IMPORTANT *** THING TO DO BEFORE RUNNING THIS? --
-------------------------------------------------------------------------
--  There is one significant change you MUST make before running the script. That is to edit the value of the "Text_File_Output_Path" variable
--    in line 59 below. You can also optionally edit the value of the "Text_File_Output_Name" and the "Show_All_Hex" toggle in the next two lines.
--    After making said edit(s), you should save this file so it will then always be ready for use on your comp.

-- ABOUT THAT Show_All_Hex VARIABLE. --
---------------------------------------
--  Reader 1.5.4 includes an embedded Lua 5.1 interpreter and the C++ side of Reader's code defines 76 functions which can be used in
--    script mode.  They range from simply counting all entries in a data file to reading and changing TGI values to reading and adding,
--    updating, or deleting properties or elements in the property reps. Most of those 76 are for the S3D entries which I've not checked.
--
--  Now, the C++ side is coded such that when obtaining the value of certain properties, it does an automatic XML lookup via the
--    "new_properties.xml" file and returns that for the most human readable version of the information. However, because we will want
--    to use this for later automated updating of property values based on the existing data, we need to know the true Hex values of the
--    stored data. Otherwise, this Lua code would be at the mercy of what could be changed in the XML code.
--
--  So, I've written an optional workaround section to force the Reader C++ side of the Lua code to return Hex values rather than the text
--    from the XML lookup as it would normally do. I'm doing it this way for my own later use where I need to check the values for setting
--    and/or replacing the OccupantGroups for use with the Expanded Tilesets mod.
--
--  The default setting is "Show_All_Hex = 0" and that'll prolly be what you want to use. You can change that to = 1 tho if you have techy
--    reasons to want the exact info as the data file itself stores them.

-- ABOUT MY COMMENTS IN THE CODE ITSELF BELOW. --
-------------------------------------------------
--  When comments are not on the same line as the code, they refer to the code below them. ;)

-- VIEWING THE TEXT OUTPUT FILE. --
-----------------------------------
--  The text output file from this script should be viewed in your fav text editor with "word wrap" turned off. If you let your editor do
--    the line wrapping, it'll look like gibberish when the data values are long. The same goes for this Lua code file.
--    Notepad++ is a good choice to view the files properly.

-- COMPLETE DETAILS FOR USING THIS SCRIPT
-----------------------------------------
-- See Cori's topic at https://community.simtropolis.com/forums/topic/759549-coris-lua-coding-explorations-for-reader-154/


--------------------------------------------------------------------------------------------------------------------------------------------------
-- Start of the actual code.                                                                                                                    --
--------------------------------------------------------------------------------------------------------------------------------------------------
function reader.main (this) -- This is the start of the function that Reader 1.5.4 is coded to recognize. Name must be exactly as written.

    ----------------------------------------------------------------------------------------------------------------------------------------------
    --       User Changeable Options - Start          --
    ----------------------------------------------------      -- Do NOT add trailing backslash to Text_File_Output_Path. It will be added by code.
    Text_File_Output_Path  = [[E:\Lua for Reader 1.5.4]]      -- Exact path to the output folder. Change the parts between the [[ and ]].
    Text_File_Output_Name  = [[x Cori's Lua Output.txt]]      -- The file name itself. Make changes between the [[ and ]].
    Show_All_Hex           = 0                                -- 0 = Use XML Text Lookup, 1 = All values returned as Hex
    ----------------------------------------------------      -- If Show_All_Hex = 1, it will still add XML text for Exemplar Type in parentheses.
    --        User Changeable Options - End           --
    ----------------------------------------------------------------------------------------------------------------------------------------------


-- ****************************************************************************************** --
-- * Nothing below this line should be changed unless you understand what you are doing. :p * --
-- ****************************************************************************************** --
--------------------------------------------------------------------------------------------------------------------------------------------------
-- Go thru every entry once and find the longest Name and ValueStr. Used to have variable width column header lines in output text.             --
--------------------------------------------------------------------------------------------------------------------------------------------------
    print ("Cori is analysing the file. Hang on...")                       -- Message on screen cause big files can take a while to process.
    Longest_Name      = 0                                                  -- Temp counter used for variable width columns in output file.
    Longest_Value_Str = 0                                                  -- Temp counter used for variable width columns in output file.
    pos = reader:entries_GetHeadPosition()                                 -- Set position pointer to top of file entries.
    while pos ~= 0 do                                                      -- Start looping thru each entry one by one.
        entry, pos = reader:entries_GetNext(pos)                           -- Read current Entry.
        flag       = reader:entry_GetFlag(entry)                           -- Tells what kind of entry it is. See Annex 1 in "ilive_script.txt".
        if (flag == 5) or (flag == 7) then                                 -- Is it a Cohort (5) or an Exemplar (7) ?
            exemplar = reader:ex_Decode(entry)                             -- Makes the data readable by the Properties functions.
            proppos = reader:exprop_GetHeadPosition(exemplar)              -- Set position pointer to top of property list.
            while proppos ~= 0 do                                          -- Start looping thru each property one by one.
                exprop, proppos = reader:exprop_GetNext(exemplar,proppos)  -- Reads the exemplar (or cohort) property at the current position.
                desc, descname  = reader:exprop_GetDesc(exprop)            -- "desc" is the Name Value returned as a decimal number.
                                                                           --   "descname" corresponds to the Name column in Reader.
                local cmDescNameStrLen = string.len(descname)              -- Check length of current descname.
                if cmDescNameStrLen > Longest_Name then                    -- Is it longer than we've seen so far?
                    Longest_Name = cmDescNameStrLen                        -- If so, remember the new longest length.
                end

                cmReadType = reader:exprop_GetType(exprop)                 -- This is what Data Type the property is.
                if Show_All_Hex == 1 then                                  -- Are we doing Hex Only output?
                    if (cmReadType == 0x100) or (cmReadType == 0x300)then  -- Check for Uint8 or Uint32. (Don't care about Bool right now.)
                        reader:exprop_SetType(exprop, 0x700)               -- Set to Sint32 to fool the C++ code into returning real Hex values.
                    end
                end

                valuestr = reader:exprop_GetValueStr(exprop)               -- The Value of the property returned as a formatted string.
                local cmValueStrLen = string.len(valuestr)                 -- Temp holder of text length.
                if cmValueStrLen > Longest_Value_Str then                  -- Is it longer than we've seen so far?
                    if cmValueStrLen > 58 then                             -- Let's not go to the Max of 1,518 it could be.
                        Longest_Value_Str = 58                             -- Cori selected to match her screen width when the valuestr is large.
                    else
                        Longest_Value_Str = cmValueStrLen                  -- Otherwise use the shorter length.
                    end
                end
            end
        end
    end
    reader:refresh()                                                       -- Resets the entire entries list cause we'll be reading it again.

--------------------------------------------------------------------------------------------------------------------------------------------------
-- End of counting routine.                                                                                                                     --
--------------------------------------------------------------------------------------------------------------------------------------------------


--------------------------------------------------------------------------------------------------------------------------------------------------
-- Main data gathering routine starts here.                                                                                                     --
--------------------------------------------------------------------------------------------------------------------------------------------------
    Open_Text_Output_File_Overwrite_Mode()                                 -- Also converts backslashes to forward slashes if used in the Path.
    Set_Total_Counters_To_Zero()                                           -- Use when counting the number of Cohorts and Exemplars in the file.

    pos = reader:entries_GetHeadPosition()                                 -- Set position pointer to top of file entries.
    while pos ~= 0 do                                                      -- Start looping thru each entry one by one.
        entry,pos = reader:entries_GetNext(pos)                            -- Read current Entry.
        flag = reader:entry_GetFlag(entry)                                 -- Tells what kind of entry it is. See Annex 1 in "ilive_script.txt".
        if (flag == 5) or (flag == 7) then                                 -- Is it a Cohort (5) or and Exemplar (7) ?
            T,G,I = reader:entry_GetTGI(entry)                             -- Same info as seen in left pane of Reader.
            Output_TGI_And_Column_Headers()                                -- Convert from decimal to hex and output.

            exemplar = reader:ex_Decode(entry)                             -- Makes the data readable by the Properties functions.
            proppos = reader:exprop_GetHeadPosition(exemplar)              -- Set position pointer to top of property list.

            It_Is_A_Parent_Cohort = 1                                      -- A toggle flag so we only output the differently formatted Parent
                                                                           --   Cohort line once per Entry.

            while proppos ~= 0 do                                          -- Start looping thru each property one by one.
                exprop, proppos = reader:exprop_GetNext(exemplar,proppos)  -- Reads the exemplar (or cohort) property at the current position.

                if It_Is_A_Parent_Cohort == 1 then                         -- If first time in this loop, output a differently formatted line.
                    TC, GC, IC = reader:ex_GetParentCohort(exemplar)       -- Get the Parent Cohort values.
                    Output_Parent_Cohort_Line()                            -- Outputs to the text file.
                    It_Is_A_Parent_Cohort = 0                              -- Toggled off after one output of this.
                end

                cmTmpRep = tonumber(reader:exprop_GetRep(exprop),16)       -- Force convert return value to be Hex. The exprop_GetRep(exprop)
                                                                           --   function returns either decimal or hex depending on its mood.

                -- Now we make it right justified within the output column allowing 3 characters total.
                cmRep = string.rep(" ", (3 - (string.len(string.format("%x", cmTmpRep))))) .. string.format("%x", cmTmpRep)

                desc, descname = reader:exprop_GetDesc(exprop)             -- "desc" is the Name Value returned as a decimal number.
                                                                           --   "descname" corresponds to the Name column in Reader.

                cmReadType = reader:exprop_GetType(exprop)                 -- This is what Data Type the property is.

                if         cmReadType == 0x100 then cmDataType = "Uint8  " -- Data Types defined in "ilive_script.txt".
                    elseif cmReadType == 0x200 then cmDataType = "Uint16 " --   This block sets the text that displays in the text output file.
                    elseif cmReadType == 0x300 then cmDataType = "Uint32 "
                    elseif cmReadType == 0x700 then cmDataType = "Sint32 "
                    elseif cmReadType == 0x800 then cmDataType = "Sint64 "
                    elseif cmReadType == 0x900 then cmDataType = "Float32"
                    elseif cmReadType == 0xB00 then cmDataType = "Bool   "
                    elseif cmReadType == 0xC00 then cmDataType = "String "
                end

                -- Special cases. When the Data Type is Bool, Uint8, or Uint32, the C++ code returns the Value as text based on XML lookup.
                --   By setting the Type to "0x700" we fool the C++ side into thinking this is a Sint32 data type and then it does
                --   not do the lookup so it returns the hex code.
                if Show_All_Hex == 1 then
                    if (cmReadType == 0x100) or (cmReadType == 0x300) or (cmReadType == 0xB00) then
                        reader:exprop_SetType(exprop, 0x700)
                    end
                end

                valuestr = reader:exprop_GetValueStr(exprop)                            -- The Value of the property returned as a formatted string.

                -- Now that we have the "valuestr", we want to set the Type back to its original Data Type. Then we re-read the valuestr
                --   so we get the XML lookup text for other entries. We only do this workaround fix when the user requests the All Hex
                --   Output and it is currently only used so the Exemplar Type can show both the Hex code and the text lookup.
                if Show_All_Hex == 1 then
                    if (cmReadType == 0x100) or (cmReadType == 0x300) or (cmReadType == 0xB00) then
                        reader:exprop_SetType(exprop,cmReadType)                        -- Put back to original Data Type.
                        if (desc == 0x00000010) then                                    -- Check if property is Exemplar Type.
                            local Hold_ValueStr = valuestr                              -- Temp copy of valuestr in Hex.
                            cmTmp_ValueStr = reader:exprop_GetValueStr(exprop)          -- Get XML lookup version.
                            if string.sub(cmTmp_ValueStr, 1, 2) == "0x" then            -- Check if XML not found.
                                cmTmp_ValueStr = "XML Text NOT Found"                   --  And say so.
                            end
                            valuestr = Hold_ValueStr .. " (" .. cmTmp_ValueStr .. ")"   -- Smoosh them together. XML text in parentheses.
                        end

                        -- Because we are fooling the C++ side of the code, it returns the value with too many preceding zeroes.
                        --   This part fixes the output display to show the proper two hex digits for Uint8 and Bool.
                        if (cmReadType == 0x100) or (cmReadType == 0xB00) then          -- Check if Uint8 and Bool.
                            local cmChopValue = string.gsub(valuestr, "0x000000", "0x") -- Strip out extra preceding zeroes.
                            valuestr = cmChopValue                                      -- Put corrected data back in valuestr.
                        end
                    end
                end

                -- This is the output line with all the property information.
                --   (It is rearranged from how Reader shows the columns for easier formatting.)
                local cmDescNameStrLen = string.len(descname)
                local cmPrintDescName = descname .. string.rep(" ", Longest_Name - cmDescNameStrLen)
                Text_File:write(" ", cmDataType, " - ", cmRep, " - 0x", Dec2Hex(desc,8)," - ", cmPrintDescName,"  = ", valuestr, "\n")
            end
            Text_File:write("\n")            -- Output a blank line.
        end
    end

    Output_Counts_Of_Cohorts_And_Exemplars() -- Write a recap of the total counts of cohorts and exemplars to the text file.

    reader:refresh()                         -- Resets the entire entries list in case we might do something else later.
    Text_File:close()                        -- Finalize the output file with EOF and closes it.
end
--------------------------------------------------------------------------------------------------------------------------------------------------
-- End of Main data gathering routine.                                                                                                          --
--------------------------------------------------------------------------------------------------------------------------------------------------


--------------------------------------------------------------------------------------------------------------------------------------------------
-- Auxiliary Functions Follow - They are in alphabetical order to make them easier to find.                                                     --
--------------------------------------------------------------------------------------------------------------------------------------------------

-- Creates a comma separated string of a decimal number and right justifies it using 6 total characters. Public code used and then tweaked.
-------------------------------------------------------------------------------------------------------------------------------------------
function Comma_Value(amount)
   local formatted = amount
   while true do
     formatted, k = string.gsub(formatted, "^(-?%d+)(%d%d%d)", '%1,%2')
     if (k == 0) then
       break
     end
   end
   local cmRightJustified = string.rep(" ", 6 - string.len(formatted)) .. formatted
   return cmRightJustified
end


-- Converts a decimal number to Hex and pre-pends zeroes as padding based on the cmLen parameter (typically 8) passed.
----------------------------------------------------------------------------------------------------------------------
function Dec2Hex(IN,cmLen)
    OUT = string.rep("0", (cmLen - (string.len(string.format("%x", IN))))) .. string.format("%x", IN)
    return OUT
end


-- Concatenate the Path and the File name. Also adds the middle backslash which will then be converted to a forward slash.
--------------------------------------------------------------------------------------------------------------------------
function Open_Text_Output_File_Overwrite_Mode()
    -- Add a backslash for the print on screen line, but will be converted to a forward slash momentarily.
    Output_File_Path_And_Name = Text_File_Output_Path .. [[\]] .. Text_File_Output_Name

    -- Show what's going on.
    if Show_All_Hex == 0 then
        print("Data with XML lookup text requested.")
    else
        print("Data as all Hex values requested.")
    end
    print("Creating text output file:", Output_File_Path_And_Name)

    -- Convert any backslashes in the Path to forward slashes or Lua will throw a hissy fit.
    Output_File_Path_And_Name = string.gsub(Output_File_Path_And_Name, [[\]], [[/]])

    -- The actual opening and creation of the output file in overwrite mode. No error checking. :o
    Text_File = io.open(Output_File_Path_And_Name, "w")
end


-- Adds two recap lines at the bottom of the output text.
---------------------------------------------------------
function Output_Counts_Of_Cohorts_And_Exemplars()
    Text_File:write("------------------------\n")
    Text_File:write("Total Cohorts   = ", Comma_Value(cmTotalCohorts), "\n")
    Text_File:write("Total Exemplars = ", Comma_Value(cmTotalExemplars), "\n")
end


-- Specially formatted output line for the Parent Cohort line cause it's different than the rest.
-------------------------------------------------------------------------------------------------
function Output_Parent_Cohort_Line()
    cmCohortDisplay = "0x" .. Dec2Hex(TC,8) .. "," .. "0x" .. Dec2Hex(GC,8) .. "," .. "0x" .. Dec2Hex(IC,8)
    Text_File:write("                              Parent Cohort", string.rep(" ", Longest_Name - 12)," = ", cmCohortDisplay, "\n")
end


-- This is the Header block for each Cohort or Exemplar.
--------------------------------------------------------
function Output_TGI_And_Column_Headers()
    -- Convert the T, G, and I from decimal to Hex and format with leading zeroes if less than 8 characters long. Output same to text file.
    Text_File:write("Entry: ", Dec2Hex(T,8), "-", Dec2Hex(G,8), "-", Dec2Hex(I,8), " - ")

    -- Here we add the type of Entry and underline it with the appropriate number of = characters.
    if (flag == 5) then                                                -- Flag 5 is for a Cohort in Reader's C++ Code.
      cmTotalCohorts = cmTotalCohorts + 1                              -- Increment Cohort counter which we output at the end of the report.
      Text_File:write("Cohort\n")
      Text_File:write("==========================================\n")
    elseif (flag == 7) then                                            -- Flag 7 is for an Exemplar in Reader's C++ Code.
      cmTotalExemplars = cmTotalExemplars + 1                          -- Increment Exemplar counter which we output at the end of the report.
      Text_File:write("Exemplar\n")
      Text_File:write("============================================\n")
    end

    -- The column headers and underlines.
    Text_File:write(" DataType  Rep   Name Value   Name", string.rep(" ", Longest_Name), "Value\n")
    Text_File:write(" --------  ---   ----------   ", string.rep("-", Longest_Name + 1), "   ", string.rep("-", Longest_Value_Str),"\n")
end


-- The variables used to count how many Cohorts and/or Exemplars there are in the data file.
--------------------------------------------------------------------------------------------
function Set_Total_Counters_To_Zero()
    cmTotalCohorts   = 0
    cmTotalExemplars = 0
end

-- EOF
