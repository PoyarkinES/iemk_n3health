SELECT p.item, n.code, n.name 
FROM procedures p 
    left join n3h_dict n on p.n3h_code = n.code 
WHERE item_id IN  
        (SELECT item_id FROM treat WHERE treat.patient_id = @patientId and treat.treat_date = @procedureDate and treat.account_id = @accountId)