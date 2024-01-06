#include <iostream>
#include <unistd.h>
#include <cstring>

void error(const char *msg) {
    perror(msg);
    exit(EXIT_FAILURE);
}

int main() {
    int pipefd[2];  // Дескрипторы канала: pipefd[0] - для чтения, pipefd[1] - для записи

    if (pipe(pipefd) == -1) {
        error("pipe");
    }

    pid_t child_pid = fork();

    if (child_pid == -1) {
        error("fork");
    } else if (child_pid == 0) {
        // Дочерний процесс
        close(pipefd[0]);  // Закрываем дескриптор для чтения

        // Запись данных в канал
        const char *message = "Привет, родительский процесс!";
        write(pipefd[1], message, strlen(message) + 1);

        close(pipefd[1]);  // Закрываем дескриптор для записи

    } else {
        // Родительский процесс
        close(pipefd[1]);  // Закрываем дескриптор для записи

        // Чтение данных из канала
        char buffer[100];
        read(pipefd[0], buffer, sizeof(buffer));
        std::cout << "Родительский процесс прочитал из канала: " << buffer << std::endl;

        close(pipefd[0]);  // Закрываем дескриптор для чтения
    }

    return 0;
}

