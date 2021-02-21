from subprocess import Popen, PIPE, TimeoutExpired, DEVNULL
from os import listdir, rename
import shutil
from os.path import join as path_join
from sys import argv
import re
from SourceCodeRunInfo import get_run_info_by_language_name, ISourceCodeRunInfo
from verdict import Verdict
from exceptions import *

TEST_DIRECTORY = 'tests'
TEMPORARY_FILENAME = 'Program'
SUFFIX_ANSWER_FILENAME = '.a'
STUDENT_USER = 'student'

class Logger:
    def __init__(self):
        self.__data = []

    def info(self, message):
        self.__data.append(message)

    def all_messages(self):
        return self.__data

log = Logger()


def __read_test(filename):
    with open(filename, 'rb') as f:
        return f.read()


def __write_test_answer(test_filename, answer):
    with open(f'{test_filename}.o', 'wb') as f:
        f.write(answer)


class TaskCodeRunner:
    def __init__(self, source_code_run_info: ISourceCodeRunInfo):
        self.__source_code_run_info = source_code_run_info
        self.__runnable_file = None

    def build(self, code_filename: str, result_filename: str):
        command = self.__source_code_run_info.format_build_command(code_filename, result_filename).split()
        log.info(f'Выполняем сборку: {" ".join(command)}')
        process = Popen(command, stdout=PIPE, stderr=PIPE)
        out, err = process.communicate()
        if process.returncode != 0:
            message = f'{out.decode()}\n{err.decode()}'
            log.info(f'Процесс вернул ошибку после сборки: {message}')
            raise CompilationException(message)

    def run(self, test_file: str):
        command = self.__source_code_run_info.format_run_command(self.__runnable_file)
        run_command = ['su', STUDENT_USER, '-c', f"{command}"]
        process = Popen(run_command, stdin=open(test_file, 'rb'), stdout=open(f'{test_file}.o', 'wb'), stderr=PIPE)
        try:
            err = process.communicate(timeout=time_limit)
        except TimeoutExpired:
            raise TimeLimitException()

        if process.returncode != 0:
            if b'PermissionError' in err[1]:
                log.info('Студент пытался прочитать тесты из папки')
                raise SecurityException()
            log.info(f'Программа завершилась с ошибкой: {err[1].decode()}')
            raise RuntimeException()

    def run_test(self, code_filename: str, test_file: str):
        if self.__runnable_file is None:
            if self.__source_code_run_info.need_build():
                if self.__source_code_run_info.file_extension() == '.java':
                    self.__runnable_file = f'{code_filename[:-5]}'
                else:
                    self.__runnable_file = TEMPORARY_FILENAME
                self.build(code_filename, self.__runnable_file)
            else:
                self.__runnable_file = code_filename

        self.run(test_file)

def is_test_file(filename):
    return re.match('^\d+$', filename) is not None


def check(source_code_run_info, code_filename):
    runner = TaskCodeRunner(source_code_run_info)
    for test_number, test_filename in enumerate(sorted(filter(is_test_file, listdir(TEST_DIRECTORY))), 1):
        log.info(f'check - Номер теста: {test_number}; Имя файла: {test_filename}')
        try:
            runner.run_test(code_filename, path_join(TEST_DIRECTORY, test_filename))
            check_command = [path_join('.', 'check'),
                             path_join(TEST_DIRECTORY, test_filename),
                             f'{path_join(TEST_DIRECTORY, test_filename)}.o',
                             path_join(TEST_DIRECTORY, f'{test_filename}{SUFFIX_ANSWER_FILENAME}')]
            process = Popen(check_command, stdout=DEVNULL, stderr=PIPE)
            _, err = process.communicate()
            log.info(f'Вердикт чеккера: {err.decode()}')
            if not err.startswith(b'ok'):
                raise WrongAnswerException()
        except CompilationException as e:
            return {
                'Verdict': Verdict.CompilationError.name,
                'CompilationOutput': e.message()
            }
        except RuntimeException:
            return {
                'Verdict': Verdict.RuntimeError.name,
                'TestNumber': test_number
            }
        except TimeLimitException:
            return {
                'Verdict': Verdict.TimeLimit.name,
                'TestNumber': test_number
            }
        except WrongAnswerException:
            return {
                'Verdict': Verdict.WrongAnswer.name,
                'TestNumber': test_number
            }
        except SecurityException:
            return {
                'Verdict': Verdict.SecurityException.name
            }
    return {
        'Verdict': Verdict.Ok.name,
    }

def remove_region_on_solution(filename):
    with open(filename, 'r') as f:
        data = f.read()

    data = re.sub('(#|\/\/)\s*(pragma\s+)?region\s+Task', '', data)
    data = re.sub('(#|\/\/)\s*(pragma\s+)?endregion\s+Task', '', data)

    with open(filename, 'w') as f:
        f.write(data)

def get_code_filename(old_filename, source_code_run_info):
    if source_code_run_info.file_extension() == '.java' :
        with open(old_filename, 'r') as f:
            data = f.read()
        match = re.search('class\s*([\w\s]+?)\s*{', data)
        if match:
            return match.group(1) + '.java'
    return old_filename.replace('.any', source_code_run_info.file_extension())

language = argv[1].lower()
time_limit = float(argv[2].replace(',', '.'))
solution_filename = 'Program.any'
rename(path_join('solutions', argv[3]), solution_filename)
shutil.rmtree('solutions')
TaskCodeRunner(get_run_info_by_language_name('cpp')).build('check.cpp', 'check')  # Скомпилировали чеккер
run_info = get_run_info_by_language_name(language)
solution_filename_by_language = get_code_filename(solution_filename, run_info)
rename(solution_filename, solution_filename_by_language)
remove_region_on_solution(solution_filename_by_language)
result = check(run_info, solution_filename_by_language)
result['Logs'] = log.all_messages()
print(result)
