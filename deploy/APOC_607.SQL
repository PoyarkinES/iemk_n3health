//10.02.2022	Vladimir Soloviev		SIR# DT00029859  File:APOC_607.SQL

Update DBA.APOC_PARAMETERS 
Set visible = (CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END)
Where param_code In ('ACTIVATE_N3H');

Update DBA.APOC_PARAMETERS 
Set visible = '2'
Where param_group_id In (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'N3HEALTH');

Insert Into DBA.APOC_Dictionary ( id, dict_id, option_code, option_name )
values (
(Select Max( id ) + 1 From DBA.APOC_Dictionary),
470,
0, 
CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Отложенная передача данных' ELSE 'Deferred data transfer' END
 );

Insert Into DBA.APOC_Dictionary ( id, dict_id, option_code, option_name )
values (
(Select Max( id ) + 1 From DBA.APOC_Dictionary),
470,
1, 
CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Диапазон' ELSE 'Range' END
 );

INSERT INTO DBA.APOC_PARAMETERS 
(
	param_id,
	param_nn,
	param_code ,
	param_name ,
	edit_mask ,
	visible ,
	start_date ,
	end_date ,
	param_datatype_id ,
	param_inputtype_id ,
	param_group_id ,
	object_type_id ,
	param_help ,
	object_type_change_taboo ,
	exclusive_edit_mode 
)
VALUES (
	(SELECT MAX(Param_ID) + 1 FROM DBA.APOC_Parameters),
	(SELECT COALESCE(max(PARAM_NN),0) + 10 FROM DBA.APOC_PARAMETERS AP1
		WHERE AP1.PARAM_GROUP_ID IN (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'N3HEALTH')),
	'N3H_TR_MODE',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Выбор периода передачи данных' ELSE 'Selecting the data transfer period' END,
	'Option_Code#APOC_Dictionary#Option_Name#Dict_ID=470',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	3,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'GL_SET'),							
	NULL,
	1,
	'N');

UPDATE APOC_Object_Groups_Parameters
   SET Default_Value = '0'
  FROM APOC_Object_Groups_Parameters,
       APOC_Parameters,
       APOC_Object_Groups,
       APOC_Object_Types
 WHERE APOC_Object_Groups_Parameters.Param_ID = APOC_Parameters.Param_ID
   AND APOC_Object_Groups_Parameters.Object_Type_ID = APOC_Object_Types.Object_Type_ID
   AND APOC_Object_Groups_Parameters.Object_Group_ID = APOC_Object_Groups.Object_Group_ID
   AND APOC_Parameters.Param_Code = 'N3H_TR_MODE'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';

//select * from APOC_Parameters where param_code like '%N3H%';

INSERT INTO DBA.APOC_PARAMETERS 
(
	param_id,
	param_nn,
	param_code ,
	param_name ,
	edit_mask ,
	visible ,
	start_date ,
	end_date ,
	param_datatype_id ,
	param_inputtype_id ,
	param_group_id ,
	object_type_id ,
	param_help ,
	object_type_change_taboo ,
	exclusive_edit_mode 
)
VALUES (
	(SELECT MAX(Param_ID) + 1 FROM DBA.APOC_Parameters),
	(SELECT COALESCE(max(PARAM_NN),0) + 10 FROM DBA.APOC_PARAMETERS AP1
		WHERE AP1.PARAM_GROUP_ID IN (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'N3HEALTH')),
	'N3H_BY_DAYS',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Минус кличество дней от текущей даты' ELSE 'Minus the number of days from the current date' END,
	'##',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	0,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'GL_SET'),							
	NULL,
	1,
	'N');

UPDATE APOC_Object_Groups_Parameters
   SET Default_Value = '0'
  FROM APOC_Object_Groups_Parameters,
       APOC_Parameters,
       APOC_Object_Groups,
       APOC_Object_Types
 WHERE APOC_Object_Groups_Parameters.Param_ID = APOC_Parameters.Param_ID
   AND APOC_Object_Groups_Parameters.Object_Type_ID = APOC_Object_Types.Object_Type_ID
   AND APOC_Object_Groups_Parameters.Object_Group_ID = APOC_Object_Groups.Object_Group_ID
   AND APOC_Parameters.Param_Code = 'N3H_BY_DAYS'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';

INSERT INTO DBA.APOC_PARAMETERS 
(
	param_id,
	param_nn,
	param_code ,
	param_name ,
	edit_mask ,
	visible ,
	start_date ,
	end_date ,
	param_datatype_id ,
	param_inputtype_id ,
	param_group_id ,
	object_type_id ,
	param_help ,
	object_type_change_taboo ,
	exclusive_edit_mode 
)
VALUES (
	(SELECT MAX(Param_ID) + 1 FROM DBA.APOC_Parameters),
	(SELECT COALESCE(max(PARAM_NN),0) + 10 FROM DBA.APOC_PARAMETERS AP1
		WHERE AP1.PARAM_GROUP_ID IN (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'N3HEALTH')),
	'N3H_PER_FROM',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Диапазон с…' ELSE 'Period from...' END,
	'dd.mm.yyyy',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	2,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'GL_SET'),							
	NULL,
	1,
	'N');

UPDATE APOC_Object_Groups_Parameters
   SET Default_Value = ''
  FROM APOC_Object_Groups_Parameters,
       APOC_Parameters,
       APOC_Object_Groups,
       APOC_Object_Types
 WHERE APOC_Object_Groups_Parameters.Param_ID = APOC_Parameters.Param_ID
   AND APOC_Object_Groups_Parameters.Object_Type_ID = APOC_Object_Types.Object_Type_ID
   AND APOC_Object_Groups_Parameters.Object_Group_ID = APOC_Object_Groups.Object_Group_ID
   AND APOC_Parameters.Param_Code = 'N3H_PER_FROM'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';

INSERT INTO DBA.APOC_PARAMETERS 
(
	param_id,
	param_nn,
	param_code ,
	param_name ,
	edit_mask ,
	visible ,
	start_date ,
	end_date ,
	param_datatype_id ,
	param_inputtype_id ,
	param_group_id ,
	object_type_id ,
	param_help ,
	object_type_change_taboo ,
	exclusive_edit_mode 
)
VALUES (
	(SELECT MAX(Param_ID) + 1 FROM DBA.APOC_Parameters),
	(SELECT COALESCE(max(PARAM_NN),0) + 10 FROM DBA.APOC_PARAMETERS AP1
		WHERE AP1.PARAM_GROUP_ID IN (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'N3HEALTH')),
	'N3H_PER_TO',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Диапазон по…' ELSE 'Period to...' END,
	'dd.mm.yyyy',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	2,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'GL_SET'),							
	NULL,
	1,
	'N');

UPDATE APOC_Object_Groups_Parameters
   SET Default_Value = ''
  FROM APOC_Object_Groups_Parameters,
       APOC_Parameters,
       APOC_Object_Groups,
       APOC_Object_Types
 WHERE APOC_Object_Groups_Parameters.Param_ID = APOC_Parameters.Param_ID
   AND APOC_Object_Groups_Parameters.Object_Type_ID = APOC_Object_Types.Object_Type_ID
   AND APOC_Object_Groups_Parameters.Object_Group_ID = APOC_Object_Groups.Object_Group_ID
   AND APOC_Parameters.Param_Code = 'N3H_PER_TO'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';

