select third_parties.code, third_parties.name 
from third_parties 
where third_parties.thp_type = @thp_type