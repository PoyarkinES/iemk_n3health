select TRIM(surname), TRIM(firstname), TRIM(middlename), birthdate, member_id, n.Code, s.snils, s.provider_no_1_id 
from staff s 
    join staff_positions sp on sp.prof_id = s.Prof_id  
    left join n3h_dict n on n.id = sp.n3h_dict_id  
where member_id =  @member_id