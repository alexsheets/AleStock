import json
import re
from urllib.request import Request, urlopen


class NasdaqAPI:
    
    # initialize class using this URL
    def __init__(self, key):
        self.URL = 'ftp://ftp.nasdaqtrader.com/symboldirectory/nasdaqtraded.txt'
        
    def retrieve_stocks(self):
        try:
            req = Request(self.URL)
            req.add_header('Accept', 'application/json, text/plain, */*')
            req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
            
            content = urlopen(req)
            data = content.read().decode('utf-8')
            
            # use regex pattern to extract tickers
            pattern = r'^\w\|(\w+)'
            symbols = []
    
            for line in content.split('\n'):
                match = re.match(pattern, line)
                if match:
                    symbols.append(match.group(1))
                    
            # write resulting tickers to json file
            with open('stocks.json', 'w') as f:
                json.dump(symbols, f)        
        except Exception as ex:
            raise Exception(f"Failed symbols: {str(ex)}")
        
    