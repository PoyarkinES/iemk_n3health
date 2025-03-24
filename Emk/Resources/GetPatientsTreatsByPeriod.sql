SELECT distinct t.patient_id, t.treat_date, t.practice_id, d.Code, d.name 
FROM treat t LEFT JOIN practice_services s on s.service_id = t.service_id 
    LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id 
WHERE (@since is null OR treat_date >= @since) AND (@to is null OR treat_date < @to) 
    AND t.ref_status is null AND lab_work_id IS NULL 
ORDER BY treat_date, patient_id
