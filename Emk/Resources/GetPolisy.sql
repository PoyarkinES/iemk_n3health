SELECT DISTINCT 
    phf.hf_plan_series series, 
    phf.hf_member_code number, 
    hfp.hf_plan_name name, 
    phf.patient_id, 
    hfp.hf_plan_code, 
    t.account_id,
    hfp.scheme_id
FROM treat t
    JOIN account_payment_plan app ON t.account_id = app.patient_account_id
    JOIN hf_plans hfp ON app.hf_plan_id = hfp.hf_plan_id
    JOIN patients_hf phf ON phf.patient_id = t.patient_id AND hfp.hf_plan_id = phf.hf_plan_id
    JOIN third_parties tp ON tp.third_party_id = hfp.hf_id
WHERE t.ref_status IS NULL 
    AND tp.thp_type = 1 AND t.account_id = @accId
ORDER BY t.account_id DESC