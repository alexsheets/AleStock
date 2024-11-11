import json
import urllib.request

class SimFinAPI:
    
    # initialize simfin class using 
    def __init__(self, key):
        # self.key = key
        # for now, input my own key 
        self.key = '44978d92-383a-4797-b25a-55a7c855cd57'
        
    def send_request_company_statements(self, ticker, statement_type, year, period):
        headers = {
            "Accept": "application/json, text/plain, */*",
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36',
            'Authorization': self.key
        }
        # create param string to append to url for GET request
        params = f'?ticker={ticker}&statements={statement_type}&year={year}&period={period}'
        # create link to backend with params appended
        req_link = 'https://backend.simfin.com/api/v3/companies/statements/compact/{}'.format(params)
        # retrieve response
        response = urllib.request.urlopen(req_link, headers=headers, params=params)  
        
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(f'Request failed, status code: {response.status_code}')
        