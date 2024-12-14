SELECT distinct t.practice_id as tpr, pa.practice_id as ppr, pa.id as acc, tpr.description , ppr.description
FROM treat t JOIN patients_accounts pa 
    JOIN practice_locations tpr ON tpr.practice_id = t.practice_id
    JOIN practice_locations ppr ON ppr.practice_id = pa.practice_id
WHERE t.account_id = @account_id