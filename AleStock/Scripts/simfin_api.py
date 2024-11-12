import json
import urllib.request

class SimFinAPI:
    
    # initialize simfin class using api key
    def __init__(self, key):
        self.key = key
        
    # func for sending request, will be used for the different types of statements
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
        
    # function for extracting the data of separate kinds of statements and returning the json
    def pull_data(self, data):
        if not data:
            raise Exception("No data found.")
        else:
            ret_data = {}
            data_extract = data[0]['statements'][0]['data'][0]
            columns = data[0]['statements'][0]['columns']
            
            for index, key in enumerate(columns):
                ret_data[key] = data_extract[index]
               
            return ret_data
        
    # create function which will aggregate the data and build a summary ubiquitously 
    def build_json(self, data, columns):
        summary = {}
        for column, keys in columns.items():
            summary[column] = {}
            for key in keys:
                val = data.get(key)
                if isinstance(val, (int, float)):
                    value = f"{value:,}"
                elif val is None:
                    value = "N/A"
                else:
                    value = val
                
                summary[column][key] = value
         
        return summary
                    


    # functions for pulling different statements and creating json data based on them

    def pull_derived_data(self, ticker, year, period):
        data = self.send_request_company_statements(ticker, "derived", year, period)
        extracted_data = self.pull_data(data)
        
    def pull_cash_data(self, ticker, year, period):
        data = self.send_request_company_statements(ticker, "cf", year, period)
        extracted_data = self.pull_data(data)
        
    def pull_profit_loss(self, ticker, year, period):
        data = self.send_request_company_statements(ticker, "pl", year, period)
        extracted_data = self.pull_data(data)
        
    def pull_balance_sheet(self, ticker, year, period):
        data = self.send_request_company_statements(ticker, "bs", year, period)
        extracted_data = self.pull_data(data)
        