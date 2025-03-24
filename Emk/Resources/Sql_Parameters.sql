WITH p_value AS (
    SELECT *
    FROM (
        SELECT Param_Code, param_value
        FROM APOC_Parameters_Values apv JOIN APOC_Parameters ap 
        WHERE Param_Code IN ('ACTIVATE_N3H','N3H_EMK_URL','N3H_PAT_URL','N3H_DATA_ON','N3H_REFR_TIME','N3H_TR_MODE','N3H_BY_DAYS','N3H_PER_FROM','N3H_PER_TO')
    ) up
    PIVOT 
    ( MIN([param_value]) 
     for Param_Code in ('ACTIVATE_N3H','N3H_EMK_URL','N3H_PAT_URL','N3H_DATA_ON','N3H_REFR_TIME','N3H_TR_MODE','N3H_BY_DAYS','N3H_PER_FROM','N3H_PER_TO')) p)
SELECT t1.object_id, t1.['N3H_KEY'] N3H_KEY, t1.['N3H_PRACTICE'] N3H_PRACTICE, 
    p_value.['ACTIVATE_N3H'] ACTIVATE_N3H, p_value.['N3H_EMK_URL'] N3H_EMK_URL, p_value.['N3H_PAT_URL'] N3H_PAT_URL, p_value.['N3H_DATA_ON'] N3H_DATA_ON, 
    p_value.['N3H_REFR_TIME'] N3H_REFR_TIME, p_value.['N3H_TR_MODE'] N3H_TR_MODE, p_value.['N3H_BY_DAYS'] N3H_BY_DAYS, p_value.['N3H_PER_FROM'] N3H_PER_FROM, 
    p_value.['N3H_PER_TO'] N3H_PER_TO
FROM(
    SELECT *
    FROM (
        SELECT Param_Code, param_value, object_id
        FROM APOC_Parameters_Values apv JOIN APOC_Parameters ap 
        WHERE Param_Code IN ('N3H_KEY','N3H_PRACTICE')
    ) up
    PIVOT 
    ( MIN([param_value]) 
     for Param_Code in ('N3H_KEY', 'N3H_PRACTICE')) p) t1, p_value
WHERE t1.['N3H_KEY'] <> ''