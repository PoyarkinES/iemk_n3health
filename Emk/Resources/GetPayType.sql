SELECT COUNT(esf.account_id) AS acc_cnt 
FROM esign_files esf
WHERE esf.account_id = @paccount AND (esf.is_sign_pr = 0 OR esf.is_sign_cmn = 0)