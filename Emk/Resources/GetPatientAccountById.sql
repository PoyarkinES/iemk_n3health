SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, 
COALESCE(ds.diagnosis_name,mkb.item,'') diagnosis_name, COALESCE(ds.diagnosis_code,mkb.code,'') diagnosis_code,
(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id) list_procedures,
DATE(esf.date_created) EsfDate 
FROM treat t JOIN procedures pr 
    LEFT JOIN esign_files esf on t.account_id = esf.account_id 
    left join n3h_dict n on pr.n3h_code = n.code 
    LEFT JOIN practice_services s on s.service_id = t.service_id 
    LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id 
    left join treat_diagnosis td on td.treat_id = t.treat_id 
    left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id 
    left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id 
    left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 
WHERE t.account_id = @account_id and t.ref_status is null AND lab_work_id IS NULL