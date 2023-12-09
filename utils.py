from abc import ABC, abstractmethod
from sched import scheduler

class ScheduleTask(ABC):
    @abstractmethod
    def create_next_task(self, schedule: scheduler, setup: bool):
        """
            This is where the task must attach itself to the schedule.
            It will also create a callback to itself, then call the run method
        """
        pass

    @abstractmethod
    def run(self):
        """
            This is where logic will be run once the allocated time is due.
        """
        pass

    @staticmethod
    @abstractmethod
    def get_tasks() -> list["ScheduleTask"]:
        """
            Process env to create a list of tasks
        """
        pass
