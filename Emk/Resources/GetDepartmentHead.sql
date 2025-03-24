select member_id, TRIM(surname) surname, TRIM(firstname) firstname, TRIM(middlename) middlename, birthdate, n.Code, s.snils, s.provider_no_1_id 
from staff s  
    join staff_positions sp on sp.prof_id = s.Prof_id  
    left join n3h_dict n on n.id = sp.n3h_dict_id  
where member_id = (Select d.manager_id from staff s join departments d on d.depart_id = s.depart_id where s.member_id = @memberId)