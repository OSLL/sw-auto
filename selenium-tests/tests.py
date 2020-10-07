import unittest
import argparse
import sys
import os
from parameterized import parameterized_class
from selenium.webdriver import Chrome
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.common.by import By
from selenium.webdriver.support.expected_conditions import url_to_be, element_to_be_clickable, invisibility_of_element

username = "admin"
password = "password"

class AuthorizedTests(unittest.TestCase):
    def setUp(self):
        # initialize Chrome browser
        options = Options()
        options.headless = True
        options.add_argument('--no-sandbox')
        options.add_argument('--window-size=1420,1080')
        options.add_argument('--allow-insecure-localhost')
        options.add_experimental_option('excludeSwitches', ['enable-logging'])
        self.driver = Chrome(options=options)

        # login
        self.driver.get("http://localhost:4444/Account/Login")  
        self.driver.find_element(By.ID, "Login").send_keys(username)
        self.driver.find_element(By.ID, "Password").send_keys(password)
        self.driver.find_element(By.CSS_SELECTOR, "form[action='/Account/Login'] input[type='submit']").click()
       

    def tearDown(self):
        self.driver.quit()


class FunctionalTests(AuthorizedTests):
    # try uploading an empty dictionary
    def test_upload_dictionary(self): 
        self.driver.get("http://localhost:4444/StudentTeacher/AddDictionary")  
        self.driver.find_element(By.ID, "Name").send_keys("dict")
        self.driver.find_element(By.ID, "UploadFile").send_keys(os.path.abspath('./test_files/dict.txt'))
        self.driver.find_element(By.CSS_SELECTOR, "form[action='/StudentTeacher/AddDictionary'] input[type='submit']").click()

    # try creating a criteria set with default values
    def test_create_criteria(self):
        self.driver.get("http://localhost:4444/StudentTeacher/TeacherAddCriterion")  
        self.driver.find_element(By.ID, "Name").send_keys("name")
        self.driver.find_element(By.ID, "Summary").send_keys("summary")
        self.driver.find_element(By.ID, "MaxScore").send_keys("100")
        self.driver.find_element(By.CSS_SELECTOR, "form[action='/StudentTeacher/TeacherAddCriterion'] input[type='submit']").click()


@parameterized_class([
    {
        "input_file": 'paper_kutsenok.docx',
        "setting_file": 'settings.json'
    },
    {
        "input_file": 'paper_kutsenok.pdf',
        "setting_file": 'settings.json'
    }
])
class TestUploadFiles(AuthorizedTests):
    # try sending 2 files to review
    def test_files(self):
        self.driver.get("http://localhost:4444/")  
        self.driver.find_element(By.ID, "loadbtn").send_keys(os.path.join(os.path.abspath('./test_files'),self.setting_file))
        self.driver.find_element(By.ID, "uploadfile").send_keys(os.path.join(os.path.abspath('./test_files'),self.input_file))
        self.driver.find_element(By.ID, "runBtn").click()


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--username', default='admin')
    parser.add_argument('--password', default='password')
    parser.add_argument('unittest_args', nargs='*')

    args = parser.parse_args()
    
    username = args.username
    password = args.password

    # Now set the sys.argv to the unittest_args (leaving sys.argv[0] alone)
    unit_argv = [sys.argv[0]] + args.unittest_args
    unittest.main(argv=unit_argv)