select patient_id, TRIM(surname), TRIM(firstname), TRIM(middlename), dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2, param_value
from patients 
    left join APOC_Parameters_Values on object_id = patient_id and param_id = (SELECT Param_ID FROM APOC_Parameters WHERE Param_Name = '—Õ»À—' and Param_Code like '%EXT%')
where patients_cart_num = @patientCartNum