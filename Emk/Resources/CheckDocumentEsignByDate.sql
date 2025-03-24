SELECT COUNT(esf.account_id) AS acc_cnt 
FROM esign_files esf 
WHERE esf.account_id = @account_id AND esf.date_sent IS NOT NULL