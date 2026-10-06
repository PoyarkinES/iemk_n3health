select top (1) COALESCE(diagnosis_name,item,'') diagnosis_name, COALESCE(diagnosis_code,code,'') diagnosis_code
from treat 
    left join treat_diagnosis 
    left join diagnoses 
    left join treat_diagnosis_mkb10 
    left join mkb10 
where (treat_diagnosis.treat_id is not null OR treat_diagnosis_mkb10.treat_id is not null) 
    and treat.patient_id = @patient_id 
    and treat.treat_date = @treat_date
order by treat.treat_id DESC