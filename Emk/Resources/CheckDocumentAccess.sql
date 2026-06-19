SELECT ef.account_id, 
    ef.date_approved, 
    ef.date_created, 
    ef.date_sent, 
    ef.efiles_name, 
    ef.efiles_path, 
    ef.esign_files_id, 
    ef.is_sign_cmn, 
    ef.is_sign_pr, 
    ef.patient_id, 
    ef.practice_id, 
    ef.provider_id, 
    ef.uuid
FROM esign_files ef WHERE account_id = @accId