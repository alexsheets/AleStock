import json
from urllib.request import Request, urlopen
from datetime import date as datemethod
from datetime import timedelta

class MarketStackAPI:
    
    # initialize marketstack class using api key
    def __init__(self, key):
        self.key = key

    def get_json_eod(self, ticker, date_start, date_end):
        params = f'?access_key={self.key}&exchange=IEXG&symbols={ticker}&date_from={date_start}&date_to={date_end}&sort=asc'
        req_link = 'https://api.marketstack.com/v1/eod{}'.format(params)
        
        req = Request(req_link)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data
    
    def get_eod_dict(data):
        ret_data = data['history']
        
    def get_json_intraday(self, ticker, interval, date_start, date_end):
        params = f'?access_key={self.key}&exchange=IEXG&symbols={ticker}&interval={interval}&date_from={date_start}&date_to={date_end}'
        req_link = 'https://api.marketstack.com/v1/intraday{}'.format(params)
        req = Request(req_link)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data
    
    def get_intra_dict(data):
        ret_data = data['intraday']
    
    def get_json_splits(self, ticker):
        params = f'?access_key={self.key}&symbols={ticker}'
        req_link = 'https://api.marketstack.com/v1/splits{}'.format(params)
        
        req = Request(req_link)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data