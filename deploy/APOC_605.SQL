//06.01.2022	Vladimir Soloviev		SIR# DT00029859  File:APOC_605.SQL

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
		WHERE AP1.PARAM_GROUP_ID IN (SELECT APG1.param_group_id FROM DBA.apoc_parameter_groups APG1 WHERE APG1.param_group_code = 'COMMUNICAT')),
	'ACTIVATE_N3H',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Интеграция с N3.Health включена' ELSE 'Activate the integration with N3.Health' END,
	'Option_Code#APOC_Dictionary#Option_Name#Dict_ID=1',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	3,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'COMMUNICAT' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
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
   AND APOC_Parameters.Param_Code = 'ACTIVATE_N3H'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';

INSERT INTO DBA.APOC_PARAMETER_GROUPS (PARAM_GROUP_ID,OBJECT_TYPE_ID,PARAM_GROUP_CODE,PARAM_GROUP_NAME) 
VALUES 
((SELECT MAX(PARAM_GROUP_ID)+1 FROM DBA.APOC_PARAMETER_GROUPS),
(SELECT OBJECT_TYPE_ID FROM DBA.APOC_OBJECT_TYPES WHERE OBJECT_TYPE_CODE='GL_SET'),
'N3HEALTH',
CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Интеграция с N3.Health ' ELSE 'N3.Health intergation ' END);

INSERT INTO DBA.APOC_PARAMETER_GROUPS (PARAM_GROUP_ID,OBJECT_TYPE_ID,PARAM_GROUP_CODE,PARAM_GROUP_NAME) 
VALUES 
((SELECT MAX(PARAM_GROUP_ID)+1 FROM DBA.APOC_PARAMETER_GROUPS),
(SELECT OBJECT_TYPE_ID FROM DBA.APOC_OBJECT_TYPES WHERE OBJECT_TYPE_CODE='PL_SET'),
'N3HEALTH',
CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Интеграция с N3.Health ' ELSE 'N3.Health intergation ' END);

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
	'N3H_KEY',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Ключ авторизации' ELSE 'Auth key' END,
	'',
	'2',
	'19000101',
	'30000101',
	1,
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
   AND APOC_Parameters.Param_Code = 'N3H_KEY'
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
	'N3H_PRACTICE',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Идентификатор практики' ELSE 'Practice ID' END,
	'',
	'2',
	'19000101',
	'30000101',
	1,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'PL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'PL_SET'),							
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
   AND APOC_Parameters.Param_Code = 'N3H_PRACTICE'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'PL_SET';

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
	'N3H_EMK_URL',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Ссылка для сервиса ЕМК' ELSE 'EMK service URL' END,
	'',
	'2',
	'19000101',
	'30000101',
	1,
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
   AND APOC_Parameters.Param_Code = 'N3H_EMK_URL'
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
	'N3H_PAT_URL',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Ссылка для сервиса Пациенты' ELSE 'Patients service URL' END,
	'',
	'2',
	'19000101',
	'30000101',
	1,
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
   AND APOC_Parameters.Param_Code = 'N3H_PAT_URL'
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
	'N3H_DATA_ON',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Активировать отправку данных' ELSE 'Activate data send' END,
	'Option_Code#APOC_Dictionary#Option_Name#Dict_ID=1',
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
   AND APOC_Parameters.Param_Code = 'N3H_DATA_ON'
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
	'N3H_REFR_TIME',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN 'Время обновления данных' ELSE 'Time to data refresh' END,
	'HH:MM',
	CASE WHEN dba.sf_get_param_spec_value('lang') = 1 THEN '1' ELSE '2' END,
	'19000101',
	'30000101',
	4,
	2,
	(SELECT AP2.Param_Group_ID FROM DBA.APOC_Parameter_Groups AP2, DBA.APOC_Object_Types OT2
		WHERE AP2.Param_Group_Code = 'N3HEALTH' AND AP2.Object_Type_ID = OT2.Object_Type_ID AND OT2.Object_Type_Code = 'GL_SET'),
	(SELECT OT3.Object_Type_ID FROM DBA.APOC_Object_Types OT3 WHERE OT3.Object_Type_Code = 'GL_SET'),							
	NULL,
	1,
	'N');

UPDATE APOC_Object_Groups_Parameters
   SET Default_Value = '21:00'
  FROM APOC_Object_Groups_Parameters,
       APOC_Parameters,
       APOC_Object_Groups,
       APOC_Object_Types
 WHERE APOC_Object_Groups_Parameters.Param_ID = APOC_Parameters.Param_ID
   AND APOC_Object_Groups_Parameters.Object_Type_ID = APOC_Object_Types.Object_Type_ID
   AND APOC_Object_Groups_Parameters.Object_Group_ID = APOC_Object_Groups.Object_Group_ID
   AND APOC_Parameters.Param_Code = 'N3H_REFR_TIME'
   AND APOC_Object_Groups.Object_Group_Code = 'D4W'
   AND APOC_Object_Types.Object_Type_Code = 'GL_SET';




