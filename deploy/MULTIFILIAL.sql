Update DBA.APOC_PARAMETERS Set param_group_id = 	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'PL_SET'),
object_type_id = (SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'PL_SET')
Where param_code In ('N3H_KEY');